using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrlandoUp;
using OrlandoUp.Application;
using OrlandoUp.Domain;
using OrlandoUp.Infrastructure.Data;
using OrlandoUp.Infrastructure.Seeding;

namespace OrlandoUp.Tests;

/// <summary>
/// The administration's write path: first the three proofs that the harness works, then what the
/// screens do to the catalog and what the public site reads back.
/// </summary>
/// <remarks>
/// The three proofs are one argument, not three tests that happen to be near each other. The first
/// says an authenticated request gets past the gate. The second says a post carrying the token is
/// accepted. The third says the same post WITHOUT the token is refused — and it is the third that
/// gives the second its meaning, because a post accepted by a host with antiforgery switched off
/// would look exactly like a post accepted because its token was right.
///
/// The form used is the sign-out of <c>_AdminLayout.cshtml</c>, which is the only authenticated
/// POST that exists before the CRUD screens do.
/// </remarks>
public class AdminCrudTests : IAsyncLifetime
{
    private readonly SiteFactory _factory = new();

    public async Task InitializeAsync() => await _factory.SeedAsync();

    public Task DisposeAsync()
    {
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task An_authenticated_request_reaches_a_page_behind_the_administration_gate()
    {
        HttpResponseMessage response = await _factory.CreateStaffClient().GetAsync("/admin/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The presence half: a 200 from a redirect chain that landed on the login page would also
        // be a 200. This asserts the page that answered is the one asked for.
        Assert.Equal("/admin/products", response.RequestMessage?.RequestUri?.AbsolutePath);
        Assert.Contains(FormPoster.TokenFieldName, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task The_same_page_without_the_seam_still_sends_the_visitor_to_the_login_page()
    {
        HttpClient anonymous = _factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

        HttpResponseMessage response = await anonymous.GetAsync("/admin/products");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.OriginalString ?? string.Empty);
    }

    [Fact]
    public async Task A_post_carrying_the_antiforgery_token_is_accepted()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        HttpResponseMessage response = await FormPoster.PostWithTokenAsync(
            staff, tokenPagePath: "/admin/products", formPath: "/admin/logout");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);

        // Equality and not containment, and the exactness is the whole assertion. A POST that never
        // got past authorization also answers Found with /admin/login in the Location — but the
        // cookie handler's challenge appends ?ReturnUrl=%2Fadmin%2Flogout, while the handler that
        // actually ran returns RedirectToPage("/Admin/Login"), whose Location is /admin/login and
        // nothing else. Containment would accept both and this test would stop naming its own name.
        Assert.Equal("/admin/login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task The_same_post_without_the_antiforgery_token_is_refused()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        // The token page is read first anyway, so the antiforgery COOKIE is present on the client
        // and the only thing missing from the post is the form field. Without this the test would
        // pass for the wrong reason: no cookie is also no token.
        await FormPoster.ReadTokenAsync(staff, "/admin/products");

        HttpResponseMessage response = await FormPoster.PostWithoutTokenAsync(staff, "/admin/logout");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =======================================================================================
    // What the write path does to the catalog, and what the public site reads back.
    //
    // Every absence assertion states a presence in the same method. A "does not contain" on its
    // own passes on a page that redirected, errored or came back empty, and would then go on
    // passing forever for the wrong reason.
    // =======================================================================================

    // ---------------------------------------------------------------------------------------
    // The store default: the form of the model, and the behaviour of a write
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void No_visibility_flag_declares_a_store_default_and_every_sentinel_is_neutral()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Type[] carriers = [typeof(Product), typeof(AddOn), typeof(DeliveryZone), typeof(DeliveryLocation)];

        foreach (Type carrier in carriers)
        {
            IProperty flag = db.Model.FindEntityType(carrier)!.FindProperty(nameof(Product.IsActive))!;

            // Read from the annotations and not from GetDefaultValue(), which answers the CLR
            // default for a property that declares nothing and would make this pass either way.
            Assert.Null(flag.FindAnnotation(RelationalAnnotationNames.DefaultValue));
            Assert.Null(flag.FindAnnotation(RelationalAnnotationNames.DefaultValueSql));

            // The half that was red before the migration: with a default declared by value, EF
            // moves the sentinel onto it and marks the property store-generated.
            Assert.Equal(false, flag.Sentinel);
            Assert.Equal(ValueGenerated.Never, flag.ValueGenerated);
        }

        // Presence, so the loop is not four passes over nothing.
        Assert.Equal(4, carriers.Length);
    }

    [Fact]
    public async Task An_explicit_false_survives_the_round_trip_on_all_four_flags()
    {
        using (IServiceScope writing = _factory.Services.CreateScope())
        {
            AppDbContext db = writing.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Products.Add(new Product { Slug = "hidden-probe", IsActive = false });
            db.AddOns.Add(new AddOn { Code = "hidden-probe", IsActive = false });
            db.DeliveryZones.Add(new DeliveryZone { Code = "hidden-probe", IsActive = false });
            db.DeliveryLocations.Add(new DeliveryLocation
            {
                Name = "Hidden probe",
                ZoneId = await db.DeliveryZones.Select(zone => zone.Id).FirstAsync(),
                IsActive = false,
            });

            await db.SaveChangesAsync();
        }

        using IServiceScope reading = _factory.Services.CreateScope();
        AppDbContext fresh = reading.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.False(await fresh.Products.Where(p => p.Slug == "hidden-probe").Select(p => p.IsActive).SingleAsync());
        Assert.False(await fresh.AddOns.Where(a => a.Code == "hidden-probe").Select(a => a.IsActive).SingleAsync());
        Assert.False(await fresh.DeliveryZones.Where(z => z.Code == "hidden-probe").Select(z => z.IsActive).SingleAsync());
        Assert.False(await fresh.DeliveryLocations.Where(l => l.Name == "Hidden probe").Select(l => l.IsActive).SingleAsync());

        // The presence half: a row written the ordinary way still reads true, so the four falses
        // above are the value that was asked for and not a column stuck at false.
        Assert.True(await fresh.Products.Where(p => p.Slug != "hidden-probe").Select(p => p.IsActive).FirstAsync());
    }

    // ---------------------------------------------------------------------------------------
    // Creating
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task A_product_created_through_the_screen_is_born_hidden()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        HttpResponseMessage response = await PostCreateAsync(staff, "new-arrival", "New arrival");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);

        Product created = await FindAsync("new-arrival");

        Assert.False(created.IsActive);

        // The presence half: the row really is the one the form asked for, so the false above is a
        // product that exists and is hidden rather than a product that was never written.
        Assert.Equal("New arrival", created.Translations.Single().Name);
        Assert.Contains($"/admin/products/edit/{created.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Creating_a_product_leaves_exactly_one_audit_row()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        await PostCreateAsync(staff, "audited-create", "Audited create");

        Product created = await FindAsync("audited-create");
        AuditEntry line = await SingleAuditLineAsync(nameof(Product), created.Id);

        Assert.Equal(AuditAction.Created, line.Action);
        Assert.Equal(TestAuthHandler.ActorEmail, line.ActorEmail);
        Assert.Contains("audited-create", line.Summary);
    }

    [Fact]
    public async Task A_duplicate_slug_is_refused_by_name_and_a_free_one_saves()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        string taken = await SlugOfFirstProductAsync();

        HttpResponseMessage refused = await PostCreateAsync(staff, taken, "Copy");

        Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
        Assert.Contains("already uses this address", await refused.Content.ReadAsStringAsync());

        HttpResponseMessage accepted = await PostCreateAsync(staff, "a-free-address", "Free");

        Assert.Equal(HttpStatusCode.Found, accepted.StatusCode);
    }

    // ---------------------------------------------------------------------------------------
    // Editing
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task The_editor_writes_what_it_was_given_and_the_public_page_reads_it()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        FormFields form = await ReadEditFormAsync(staff, product.Id);
        form.Set("EnglishName", "Renamed by the administration");

        HttpResponseMessage saved = await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);

        string publicList = await _factory.CreateClient().GetStringAsync("/rentals");

        Assert.Contains("Renamed by the administration", publicList);
    }

    [Fact]
    public async Task Saving_a_product_stamps_the_moment_of_the_edit()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        Assert.Null(product.UpdatedAtUtc);

        FormFields form = await ReadEditFormAsync(staff, product.Id);

        await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Product after = await FindAsync(product.Slug);

        Assert.NotNull(after.UpdatedAtUtc);
        Assert.Equal(after.CreatedAtUtc.Kind, after.UpdatedAtUtc!.Value.Kind);
    }

    [Fact]
    public async Task Editing_a_product_leaves_exactly_one_audit_row()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        FormFields form = await ReadEditFormAsync(staff, product.Id);

        await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        AuditEntry line = await SingleAuditLineAsync(nameof(Product), product.Id);

        Assert.Equal(AuditAction.Updated, line.Action);
        Assert.Contains(product.Slug, line.Summary);
    }

    [Fact]
    public async Task An_absent_dimension_survives_the_round_trip_and_the_badge_stays_off()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        Assert.NotNull(product.WidthIn);

        FormFields form = await ReadEditFormAsync(staff, product.Id);
        form.Set("WidthIn", string.Empty);

        await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Product after = await FindAsync(product.Slug);

        Assert.Null(after.WidthIn);
        Assert.Null(after.FitsDisneyTransport);

        // The presence half: the length the form did carry is still there, so the null width is an
        // emptied field and not a whole row that failed to save.
        Assert.NotNull(after.LengthIn);

        string page = await _factory.CreateClient().GetStringAsync($"/rentals/{after.Slug}");

        Assert.DoesNotContain("badge--transport", page);
        Assert.Contains(after.Slug, page);
    }

    [Fact]
    public async Task The_english_name_cannot_be_blanked()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        FormFields form = await ReadEditFormAsync(staff, product.Id);
        form.Set("EnglishName", "   ");

        HttpResponseMessage refused = await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
        Assert.Contains("name in English is required", await refused.Content.ReadAsStringAsync());

        Product after = await FindAsync(product.Slug);

        Assert.NotEmpty(after.Translations.Single(t => t.Culture == SiteCultures.English).Name);
    }

    [Fact]
    public async Task Highlights_round_trip_as_lines_and_the_awkward_characters_come_back_intact()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        string[] written =
        [
            """A "quoted" highlight""",
            @"A back\slash",
            "Under 30 in wide & fits <the> bus",
        ];

        FormFields form = await ReadEditFormAsync(staff, product.Id);
        form.Set("EnglishHighlights", string.Join("\n", written) + "\n\n   \n");

        await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Product after = await FindAsync(product.Slug);
        IReadOnlyList<string> read = CatalogWriter.ReadHighlights(
            after.Translations.Single(t => t.Culture == SiteCultures.English).Highlights);

        // Three lines and not five: the blank line and the line of spaces are not highlights.
        Assert.Equal(written, read);
    }

    [Fact]
    public async Task A_portuguese_block_left_entirely_blank_removes_the_translation_row()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        Assert.Equal(2, product.Translations.Count);

        FormFields form = await ReadEditFormAsync(staff, product.Id);
        form.Set("PortugueseName", string.Empty)
            .Set("PortugueseTagline", string.Empty)
            .Set("PortugueseDescription", string.Empty)
            .Set("PortugueseHighlights", string.Empty);

        await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Product after = await FindAsync(product.Slug);

        Assert.Single(after.Translations);
        Assert.Equal(SiteCultures.English, after.Translations.Single().Culture);
    }

    [Fact]
    public async Task A_hidden_product_stays_on_the_administration_list_and_leaves_the_public_one()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        FormFields form = await ReadEditFormAsync(staff, product.Id);
        form.Set("IsActive", "false");

        await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Assert.False((await FindAsync(product.Slug)).IsActive);

        string adminList = await staff.GetStringAsync("/admin/products");
        string publicList = await _factory.CreateClient().GetStringAsync("/rentals");

        Assert.Contains(product.Slug, adminList);
        Assert.DoesNotContain(product.Slug, publicList);
    }

    // ---------------------------------------------------------------------------------------
    // The add-on picker, which the visual check caught showing an identifier to a person
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task The_add_on_picker_shows_the_translated_name_and_not_the_internal_code()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        string html = await staff.GetStringAsync(EditPath(product.Id));

        // The presence half first: the checkbox for that add-on is on the page, so the absence
        // below is a label that changed and not a control that vanished.
        int cupHolderId = await AddOnIdAsync("cup-holder");

        Assert.Contains($"value=\"{cupHolderId}\"", html);
        Assert.Contains("Cup holder", html);
        Assert.DoesNotContain(">cup-holder<", html);
    }

    [Fact]
    public async Task An_add_on_with_no_translation_at_all_falls_back_to_its_code()
    {
        using (IServiceScope writing = _factory.Services.CreateScope())
        {
            AppDbContext db = writing.ServiceProvider.GetRequiredService<AppDbContext>();

            // No translation row in either culture: the picker answers nothing, and a blank label
            // would be a checkbox with no meaning. The code is the honest last resort.
            db.AddOns.Add(new AddOn { Code = "untranslated-extra", SortOrder = 99 });

            await db.SaveChangesAsync();
        }

        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        string html = await staff.GetStringAsync(EditPath(product.Id));

        Assert.Contains("untranslated-extra", html);

        // And the ordinary case is untouched by the fallback.
        Assert.Contains("Cup holder", html);
    }

    // ---------------------------------------------------------------------------------------
    // The price list — one test per problem a screen can produce
    // ---------------------------------------------------------------------------------------

    [Theory]
    [InlineData(PricingTierSetProblem.Empty, "at least one price band")]
    [InlineData(PricingTierSetProblem.InvalidBand, "amount of zero or less")]
    [InlineData(PricingTierSetProblem.DoesNotStartAtOneDay, "does not start at one day")]
    [InlineData(PricingTierSetProblem.Overlap, "cover the same rental length")]
    [InlineData(PricingTierSetProblem.Gap, "has no price")]
    [InlineData(PricingTierSetProblem.NoOpenEndedBand, "Every band has an upper limit")]
    public async Task A_product_on_sale_is_refused_with_a_broken_price_list(
        PricingTierSetProblem problem,
        string expectedMessage)
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();
        int bandsBefore = product.PricingTiers.Count;

        FormFields form = await ReadEditFormAsync(staff, product.Id);

        ClearTierRows(form);
        WriteBands(form, BandsFor(problem));

        HttpResponseMessage refused = await FormPoster.PostAsync(staff, EditPath(product.Id), form);
        string body = await refused.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
        Assert.Contains(expectedMessage, body);

        // Nothing was saved and the typing came back (K3 a): the bands on the row are untouched and
        // the name the operator had in the field is still in the field.
        Product after = await FindAsync(product.Slug);

        Assert.Equal(bandsBefore, after.PricingTiers.Count);
        Assert.Contains(form.Value("EnglishName")!, body);
    }

    [Fact]
    public async Task A_valid_price_list_saves_and_replaces_the_bands()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        FormFields form = await ReadEditFormAsync(staff, product.Id);

        ClearTierRows(form);
        WriteBands(form, [(1, 3, 40m), (4, null, 25m)]);

        HttpResponseMessage saved = await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);

        Product after = await FindAsync(product.Slug);

        Assert.Equal(2, after.PricingTiers.Count);
        Assert.Equal(PricingTierSetProblem.None, PricingTierRules.Validate(after.PricingTiers));
    }

    [Fact]
    public async Task A_product_that_is_not_on_sale_is_refused_a_price_band()
    {
        HttpClient staff = _factory.CreateStaffClient();
        Product product = await FirstBookableAsync();

        FormFields form = await ReadEditFormAsync(staff, product.Id);
        form.Set("IsBookable", "false");

        HttpResponseMessage refused = await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
        Assert.Contains("carries no price band", await refused.Content.ReadAsStringAsync());

        // The presence half: it is the bands that are refused, not the flag itself — clearing them
        // and the links lets the very same product leave the sale.
        ClearTierRows(form);
        form.Remove("AddOnIds");

        HttpResponseMessage accepted = await FormPoster.PostAsync(staff, EditPath(product.Id), form);

        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.False((await FindAsync(product.Slug)).IsBookable);
    }

    // ---------------------------------------------------------------------------------------
    // The fleet
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task A_duplicate_asset_tag_is_refused_as_validation_and_a_different_one_saves()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        string taken = await FirstAssetTagAsync();
        int productId = (await FirstBookableAsync()).Id;

        HttpResponseMessage refused = await PostUnitAsync(staff, taken, productId);

        Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
        Assert.Contains("already carries this asset tag", await refused.Content.ReadAsStringAsync());

        HttpResponseMessage accepted = await PostUnitAsync(staff, "FREE-TAG-001", productId);

        Assert.Equal(HttpStatusCode.Found, accepted.StatusCode);

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(1, await db.Units.CountAsync(unit => unit.AssetTag == "FREE-TAG-001"));
    }

    [Fact]
    public async Task Creating_a_unit_leaves_exactly_one_audit_row()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);
        int productId = (await FirstBookableAsync()).Id;

        await PostUnitAsync(staff, "AUDITED-001", productId);

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Unit unit = await db.Units.SingleAsync(u => u.AssetTag == "AUDITED-001");
        AuditEntry line = await SingleAuditLineAsync(nameof(Unit), unit.Id);

        Assert.Equal(AuditAction.Created, line.Action);
        Assert.Contains("AUDITED-001", line.Summary);
    }

    [Fact]
    public async Task Moving_a_unit_to_another_product_is_recorded_with_both_products_named()
    {
        HttpClient staff = _factory.CreateStaffClient();

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Unit unit = await db.Units.Include(u => u.Product).OrderBy(u => u.Id).FirstAsync();
        string leftSlug = unit.Product!.Slug;
        Product joined = await db.Products
            .Where(product => product.Id != unit.ProductId)
            .OrderBy(product => product.SortOrder)
            .FirstAsync();

        FormFields form = await FormPoster.ReadFormAsync(staff, $"/admin/units/edit/{unit.Id}");
        form.Set("ProductId", joined.Id.ToString());

        HttpResponseMessage saved = await FormPoster.PostAsync(staff, $"/admin/units/edit/{unit.Id}", form);

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);

        AuditEntry line = await SingleAuditLineAsync(nameof(Unit), unit.Id);

        Assert.Equal(AuditAction.Updated, line.Action);
        Assert.Contains(leftSlug, line.Summary);
        Assert.Contains(joined.Slug, line.Summary);
    }

    [Fact]
    public async Task The_record_screen_shows_the_line_a_write_left()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        string empty = await _factory.CreateStaffClient().GetStringAsync("/admin/audit");

        Assert.Contains("Nothing has been recorded yet", empty);

        await PostCreateAsync(staff, "shows-on-the-record", "Shows on the record");

        string after = await _factory.CreateStaffClient().GetStringAsync("/admin/audit");

        Assert.Contains("shows-on-the-record", after);
        Assert.Contains(TestAuthHandler.ActorEmail, after);
    }

    // ---------------------------------------------------------------------------------------
    // Resource keys the screens build by interpolation, which the parity test cannot see
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void Every_enum_member_a_screen_names_has_a_key_in_both_cultures()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IStringLocalizer<SharedResource> text =
            scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();

        List<string> keys = [];

        keys.AddRange(Enum.GetNames<UnitStatus>().Select(name => $"Admin_Status{name}"));
        keys.AddRange(Enum.GetNames<SeatConfiguration>().Select(name => $"Admin_Seat{name}"));
        keys.AddRange(Enum.GetNames<TierMode>().Select(name => $"Admin_Tier{name}"));
        keys.AddRange(Enum.GetNames<AuditAction>().Select(name => $"Admin_Action{name}"));
        keys.AddRange(Enum.GetNames<BatteryKind>().Select(name => $"Admin_BatteryKind{name}"));

        // The screens build these key names with string interpolation, so LocalizationParityTests,
        // which compares the two files against each other, cannot notice one that nobody wrote:
        // both files would simply be missing it. This is the assertion that does notice.
        List<string> missing = keys.Where(key => text[key].ResourceNotFound).ToList();

        Assert.Empty(missing);
        Assert.Equal(15, keys.Count);
    }


    // =======================================================================================
    // The fleet's batteries and the operational settings (leva 04b).
    //
    // The settings row is ARRANGED here and never expected (EMENDA-04B-02 B1): the suite builds
    // its schema from the model with EnsureCreatedAsync and never runs a migration, so the row the
    // migration creates does not exist in this host. That the MIGRATION creates it is proved in
    // the P1 report and again by item 8 of the conference roteiro, against the real database. What
    // the suite can prove is the other half — that the screen edits and never creates or deletes.
    // =======================================================================================

    [Fact]
    public async Task The_seed_writes_twelve_batteries_and_running_it_twice_changes_nothing()
    {
        await RunBatterySeedAsync();

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(12, await db.Batteries.CountAsync());
        Assert.Equal(1, await db.Batteries.CountAsync(b => b.Kind == BatteryKind.ExtendedRange));
        Assert.Equal(11, await db.Batteries.CountAsync(b => b.Kind == BatteryKind.Normal));

        // Six per model, and the Extended Range one is on the FIRST scooter by display order —
        // which is what EMENDA-04B-03 C1 is about, and the tie-break is what makes it repeatable.
        List<Product> scooters = await db.Products
            .Where(p => p.Category == ProductCategory.MobilityScooter)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .ToListAsync();

        Assert.Equal(6, await db.Batteries.CountAsync(b => b.ProductId == scooters[0].Id));
        Assert.Equal(6, await db.Batteries.CountAsync(b => b.ProductId == scooters[1].Id));
        Assert.Equal(
            scooters[0].Id,
            await db.Batteries.Where(b => b.Kind == BatteryKind.ExtendedRange).Select(b => b.ProductId).SingleAsync());

        // Run twice, and the second run inserts into a table that is no longer empty.
        await RunBatterySeedAsync();

        Assert.Equal(12, await db.Batteries.CountAsync());
    }

    [Fact]
    public async Task Every_seeded_tag_carries_the_shape_D38_fixed_and_the_grade_is_not_in_it()
    {
        await RunBatterySeedAsync();

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        List<string> tags = await db.Batteries.Select(b => b.AssetTag).OrderBy(tag => tag).ToListAsync();

        Assert.All(tags, tag => Assert.Matches("^B(SC|SP)-[0-9]{2}$", tag));

        // The grade is NOT in the tag (D38): BSC-06 says nothing about being the extended-range
        // one, and the Kind column is the only thing that does. This asserts both halves — that
        // the tag is the expected one, and that nothing had to read the text to know the grade.
        string extended = await db.Batteries
            .Where(b => b.Kind == BatteryKind.ExtendedRange)
            .Select(b => b.AssetTag)
            .SingleAsync();

        Assert.Equal("BSC-06", extended);
        Assert.Equal(12, tags.Count);
    }

    [Fact]
    public async Task A_duplicate_battery_tag_is_refused_as_validation_and_a_different_one_saves()
    {
        await RunBatterySeedAsync();

        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);
        int scooterId = await FirstScooterIdAsync();

        HttpResponseMessage refused = await PostBatteryAsync(staff, "BSC-01", scooterId);

        Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
        Assert.Contains("already carries this asset tag", await refused.Content.ReadAsStringAsync());

        HttpResponseMessage accepted = await PostBatteryAsync(staff, "BSC-07", scooterId);

        Assert.Equal(HttpStatusCode.Found, accepted.StatusCode);

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(13, await db.Batteries.CountAsync());
    }

    [Fact]
    public async Task A_battery_cannot_be_attached_to_a_product_that_is_not_a_scooter()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        int wheelchairId;
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            wheelchairId = await db.Products
                .Where(p => p.Category != ProductCategory.MobilityScooter)
                .OrderBy(p => p.SortOrder)
                .Select(p => p.Id)
                .FirstAsync();
        }

        HttpResponseMessage refused = await PostBatteryAsync(staff, "BSC-90", wheelchairId);

        Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
        Assert.Contains("belongs to a scooter model", await refused.Content.ReadAsStringAsync());

        // The presence half: the same post against a scooter saves, so the refusal is about the
        // category and not about the form.
        HttpResponseMessage accepted = await PostBatteryAsync(staff, "BSC-90", await FirstScooterIdAsync());

        Assert.Equal(HttpStatusCode.Found, accepted.StatusCode);
    }

    [Fact]
    public async Task An_absent_battery_range_survives_the_round_trip()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        FormFields form = await FormPoster.ReadFormAsync(staff, "/admin/batteries/create");

        form.Set("AssetTag", "BSC-91")
            .Set("ProductId", (await FirstScooterIdAsync()).ToString())
            .Set("RangeMiles", string.Empty);

        Assert.Equal(HttpStatusCode.Found, (await FormPoster.PostAsync(staff, "/admin/batteries/create", form)).StatusCode);

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Battery written = await db.Batteries.SingleAsync(b => b.AssetTag == "BSC-91");

        Assert.Null(written.RangeMiles);

        // The presence half: the row exists and carries what the form did give it, so the null is
        // an emptied field and not a save that never happened.
        Assert.Equal("BSC-91", written.AssetTag);

        // And it comes BACK empty rather than as a zero.
        string html = await _factory.CreateStaffClient().GetStringAsync($"/admin/batteries/edit/{written.Id}");
        FormFields reopened = FormFields.ReadFrom(html);

        Assert.Equal(string.Empty, reopened.Value("RangeMiles"));
    }

    [Fact]
    public async Task Retiring_a_battery_keeps_it_on_the_list_and_drops_the_available_count()
    {
        await RunBatterySeedAsync();

        HttpClient staff = _factory.CreateStaffClient();

        int id;
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            id = await db.Batteries.Where(b => b.AssetTag == "BSC-01").Select(b => b.Id).SingleAsync();
        }

        string before = await staff.GetStringAsync("/admin/batteries");

        Assert.Contains("<strong>12</strong>", before);

        FormFields form = await FormPoster.ReadFormAsync(staff, $"/admin/batteries/edit/{id}");
        form.Set("Status", nameof(UnitStatus.Retired));

        Assert.Equal(HttpStatusCode.OK, (await FormPoster.PostAsync(staff, $"/admin/batteries/edit/{id}", form)).StatusCode);

        string after = await staff.GetStringAsync("/admin/batteries");

        // Still listed, and marked. A battery that vanished from the screen is a battery somebody
        // buys twice.
        Assert.Contains("BSC-01", after);
        Assert.Contains("<strong>11</strong>", after);

        using IServiceScope reading = _factory.Services.CreateScope();
        AppDbContext fresh = reading.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(UnitStatus.Retired, await fresh.Batteries.Where(b => b.Id == id).Select(b => b.Status).SingleAsync());
    }

    [Fact]
    public async Task Creating_a_battery_leaves_exactly_one_audit_row()
    {
        HttpClient staff = _factory.CreateStaffClient(allowAutoRedirect: false);

        await PostBatteryAsync(staff, "BSC-92", await FirstScooterIdAsync());

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Battery battery = await db.Batteries.SingleAsync(b => b.AssetTag == "BSC-92");
        AuditEntry line = await SingleAuditLineAsync(nameof(Battery), battery.Id);

        Assert.Equal(AuditAction.Created, line.Action);
        Assert.Equal(TestAuthHandler.ActorEmail, line.ActorEmail);
        Assert.Contains("BSC-92", line.Summary);
    }

    [Fact]
    public async Task Editing_a_battery_leaves_exactly_one_audit_row()
    {
        await RunBatterySeedAsync();

        HttpClient staff = _factory.CreateStaffClient();

        int id;
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            id = await db.Batteries.Where(b => b.AssetTag == "BSP-03").Select(b => b.Id).SingleAsync();
        }

        FormFields form = await FormPoster.ReadFormAsync(staff, $"/admin/batteries/edit/{id}");
        form.Set("SerialNumber", "SN-EDITED");

        await FormPoster.PostAsync(staff, $"/admin/batteries/edit/{id}", form);

        AuditEntry line = await SingleAuditLineAsync(nameof(Battery), id);

        Assert.Equal(AuditAction.Updated, line.Action);
        Assert.Contains("BSP-03", line.Summary);
    }

    [Fact]
    public async Task The_settings_screen_edits_the_row_and_leaves_exactly_one_audit_line()
    {
        await ArrangeSettingsRowAsync();

        HttpClient staff = _factory.CreateStaffClient();

        FormFields form = await FormPoster.ReadFormAsync(staff, "/admin/settings");
        form.Set("LostChargerFee", "35.00");

        Assert.Equal(HttpStatusCode.OK, (await FormPoster.PostAsync(staff, "/admin/settings", form)).StatusCode);

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Exactly one row before and after: the screen edits, and a second row is not reachable
        // through it. Control C03 states the same thing statically over the pages folder.
        OperationalSettings row = await db.OperationalSettings.SingleAsync();

        Assert.Equal(35.00m, row.LostChargerFee);
        Assert.Equal(14, row.ChargerCount);
        Assert.NotNull(row.UpdatedAtUtc);

        AuditEntry line = await SingleAuditLineAsync(nameof(OperationalSettings), OperationalSettings.SingletonId);

        Assert.Equal(AuditAction.Updated, line.Action);
    }

    [Fact]
    public async Task A_negative_charger_count_and_a_negative_amount_are_refused_and_zero_is_accepted()
    {
        await ArrangeSettingsRowAsync();

        HttpClient staff = _factory.CreateStaffClient();

        FormFields negativeCount = await FormPoster.ReadFormAsync(staff, "/admin/settings");
        negativeCount.Set("ChargerCount", "-1");

        Assert.Contains(
            "charger count cannot be negative",
            await (await FormPoster.PostAsync(staff, "/admin/settings", negativeCount)).Content.ReadAsStringAsync());

        FormFields negativeAmount = await FormPoster.ReadFormAsync(staff, "/admin/settings");
        negativeAmount.Set("SecondBatteryPerDay", "-5.00");

        Assert.Contains(
            "amount cannot be negative",
            await (await FormPoster.PostAsync(staff, "/admin/settings", negativeAmount)).Content.ReadAsStringAsync());

        // Zero is legitimate: a courtesy battery is zero, and it still consumes one from the pool.
        FormFields zero = await FormPoster.ReadFormAsync(staff, "/admin/settings");
        zero.Set("SecondBatteryPerDay", "0.00");

        Assert.Equal(HttpStatusCode.OK, (await FormPoster.PostAsync(staff, "/admin/settings", zero)).StatusCode);

        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(0m, await db.OperationalSettings.Select(row => row.SecondBatteryPerDay).SingleAsync());
    }

    [Fact]
    public void No_battery_column_declares_a_store_default()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        IEntityType battery = db.Model.FindEntityType(typeof(Battery))!;
        IEntityType settings = db.Model.FindEntityType(typeof(OperationalSettings))!;

        List<IProperty> columns = battery.GetProperties().Concat(settings.GetProperties()).ToList();

        foreach (IProperty column in columns)
        {
            // Read from the annotations and never from GetDefaultValue(), which answers the CLR
            // default for a property that declares nothing and would pass either way — the lesson
            // of D35 and of the leva 04 A1 pair.
            Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));
            Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValueSql));
        }

        // The presence half: the loop really walked the two tables' columns.
        Assert.Equal(15, columns.Count);
    }


    // ---------------------------------------------------------------------------------------
    // The dashboard's one absent-able number (EMENDA-04B-04 D1)
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void The_dashboard_charger_count_is_nullable_so_absence_cannot_become_zero()
    {
        // A type assertion, because a type is a barrier and a habit is not. Projected onto a
        // non-nullable int, FirstOrDefault answers 0 for a missing row and the screen would state
        // that the operation owns no chargers — a claim about the fleet, not about a missing row.
        //
        // No control catches this: C17 of foundation.tsv is labelled "an absent price never
        // coalesces to zero" and its operand names two forms, ?? 0 and GetValueOrDefault(. A
        // value-type projection through FirstOrDefault is a third member of the same class, and
        // C17 measures 0 with the defect present. Widening that grep would separate a value-type
        // projection from a reference-type one by shape, which is how a control ends up false
        // green — eleven of the twelve FirstOrDefault calls in src/ land on entities or strings
        // and are right as they stand. So the instrument is here.
        Assert.Equal(
            typeof(int?),
            typeof(OrlandoUp.Pages.Admin.IndexModel).GetProperty(nameof(OrlandoUp.Pages.Admin.IndexModel.ChargerCount))!
                .PropertyType);
    }

    [Fact]
    public async Task The_dashboard_shows_the_chargers_as_not_set_when_the_settings_row_is_missing()
    {
        // The test host is the one environment where the row genuinely is absent: the suite builds
        // its schema from the model and never runs the migration that creates it (EMENDA-04B-02
        // B1). On Rod's database the row exists, so item 0 of the roteiro passes either way — which
        // is exactly why this is carried by a test and not by the eye.
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.False(await db.OperationalSettings.AnyAsync());
        }

        string html = await _factory.CreateStaffClient().GetStringAsync("/admin");

        Assert.Contains("""<span class="stat__value todo" id="stat-chargers">""", html);
        Assert.DoesNotContain("""<span class="stat__value" id="stat-chargers">0</span>""", html);

        // The presence half, and it is the distinction the whole item is about: an empty Batteries
        // table honestly holds zero, so THAT stat does read 0 on the same page. Absence and zero
        // are different answers and the dashboard now gives each one its own.
        Assert.Contains("""<span class="stat__value">0</span>""", html);

        // And with the row in place the same stat carries the number, so the marker above is the
        // absence and not a stat that stopped rendering.
        await ArrangeSettingsRowAsync();

        string filled = await _factory.CreateStaffClient().GetStringAsync("/admin");

        Assert.Contains("""<span class="stat__value" id="stat-chargers">14</span>""", filled);
    }

    // ---------------------------------------------------------------------------------------
    // Helpers for the battery assertions
    // ---------------------------------------------------------------------------------------

    private async Task RunBatterySeedAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();

        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IClock clock = scope.ServiceProvider.GetRequiredService<IClock>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Tests");

        Assert.Equal(0, await BatterySeeder.RunAsync(db, clock, logger, CancellationToken.None));
    }

    /// <summary>
    /// Puts the single settings row in place for the tests that edit it. The MIGRATION is what
    /// creates it in a real database; this host never runs one, so the row is arranged rather than
    /// expected (EMENDA-04B-02 B1). The values are the ones the migration writes.
    /// </summary>
    private async Task ArrangeSettingsRowAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.OperationalSettings.AnyAsync())
        {
            return;
        }

        db.OperationalSettings.Add(new OperationalSettings
        {
            Id = OperationalSettings.SingletonId,
            ChargerCount = 14,
            SecondBatteryPerDay = 8.00m,
            LostChargerFee = 30.00m,
        });

        await db.SaveChangesAsync();
    }

    private async Task<int> FirstScooterIdAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Products
            .Where(p => p.Category == ProductCategory.MobilityScooter)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Select(p => p.Id)
            .FirstAsync();
    }

    private static async Task<HttpResponseMessage> PostBatteryAsync(HttpClient staff, string tag, int productId)
    {
        FormFields form = await FormPoster.ReadFormAsync(staff, "/admin/batteries/create");

        form.Set("AssetTag", tag).Set("ProductId", productId.ToString());

        return await FormPoster.PostAsync(staff, "/admin/batteries/create", form);
    }

    // ---------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------

    private static string EditPath(int id) => $"/admin/products/edit/{id}";

    private static async Task<HttpResponseMessage> PostCreateAsync(
        HttpClient staff,
        string slug,
        string englishName)
    {
        FormFields form = await FormPoster.ReadFormAsync(staff, "/admin/products/create");

        form.Set("Slug", slug).Set("EnglishName", englishName).Set("SortOrder", "99");

        return await FormPoster.PostAsync(staff, "/admin/products/create", form);
    }

    private static async Task<HttpResponseMessage> PostUnitAsync(HttpClient staff, string tag, int productId)
    {
        FormFields form = await FormPoster.ReadFormAsync(staff, "/admin/units/create");

        form.Set("AssetTag", tag).Set("ProductId", productId.ToString());

        return await FormPoster.PostAsync(staff, "/admin/units/create", form);
    }

    private static Task<FormFields> ReadEditFormAsync(HttpClient staff, int productId) =>
        FormPoster.ReadFormAsync(staff, EditPath(productId));

    private static void ClearTierRows(FormFields form)
    {
        for (int row = 0; row < 12; row++)
        {
            form.Remove($"Tiers[{row}].MinDays")
                .Remove($"Tiers[{row}].MaxDays")
                .Remove($"Tiers[{row}].Mode")
                .Remove($"Tiers[{row}].Amount");
        }
    }

    private static void WriteBands(FormFields form, IReadOnlyList<(int? Min, int? Max, decimal? Amount)> bands)
    {
        for (int row = 0; row < bands.Count; row++)
        {
            (int? min, int? max, decimal? amount) = bands[row];

            form.Set($"Tiers[{row}].MinDays", min is int m ? m.ToString() : string.Empty)
                .Set($"Tiers[{row}].MaxDays", max is int x ? x.ToString() : string.Empty)
                .Set($"Tiers[{row}].Mode", nameof(TierMode.PerDay))
                .Set($"Tiers[{row}].Amount", amount is decimal a ? a.ToString("0.00") : string.Empty);
        }
    }

    private static IReadOnlyList<(int? Min, int? Max, decimal? Amount)> BandsFor(PricingTierSetProblem problem) =>
        problem switch
        {
            PricingTierSetProblem.Empty => [],
            PricingTierSetProblem.InvalidBand => [(1, null, 0m)],
            PricingTierSetProblem.DoesNotStartAtOneDay => [(2, null, 30m)],
            PricingTierSetProblem.Overlap => [(1, 5, 30m), (3, null, 20m)],
            PricingTierSetProblem.Gap => [(1, 2, 30m), (5, null, 20m)],
            PricingTierSetProblem.NoOpenEndedBand => [(1, 2, 30m), (3, 9, 20m)],
            _ => [],
        };

    private async Task<Product> FindAsync(string slug)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Products
            .AsNoTracking()
            .Include(product => product.Translations)
            .Include(product => product.PricingTiers)
            .Include(product => product.AddOns)
            .SingleAsync(product => product.Slug == slug);
    }

    private async Task<Product> FirstBookableAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Product product = await db.Products
            .AsNoTracking()
            .Include(p => p.Translations)
            .Include(p => p.PricingTiers)
            .Where(p => p.IsBookable && p.WidthIn != null && p.LengthIn != null)
            .OrderBy(p => p.SortOrder)
            .FirstAsync();

        return product;
    }

    private async Task<string> SlugOfFirstProductAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Products.OrderBy(product => product.SortOrder).Select(product => product.Slug).FirstAsync();
    }

    private async Task<int> AddOnIdAsync(string code)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.AddOns.Where(addOn => addOn.Code == code).Select(addOn => addOn.Id).SingleAsync();
    }

    private async Task<string> FirstAssetTagAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Units.OrderBy(unit => unit.Id).Select(unit => unit.AssetTag).FirstAsync();
    }

    private async Task<AuditEntry> SingleAuditLineAsync(string entityType, int entityId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        List<AuditEntry> lines = await db.AuditEntries
            .AsNoTracking()
            .Where(entry => entry.EntityType == entityType && entry.EntityId == entityId)
            .ToListAsync();

        // Exactly one, not at least one: a handler that recorded twice would be as wrong as one
        // that recorded nothing, and only this shape catches it.
        return Assert.Single(lines);
    }
}
