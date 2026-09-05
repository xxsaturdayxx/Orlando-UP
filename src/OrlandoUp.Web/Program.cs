using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OrlandoUp.Api;
using OrlandoUp.Application;
using OrlandoUp.Infrastructure;
using OrlandoUp.Infrastructure.Data;
using OrlandoUp.Infrastructure.Localization;
using OrlandoUp.Infrastructure.Seeding;

// The culture used to FORMAT every number, money amount and date is always this one (D20).
// Only the culture used to CHOOSE the text moves between en-US and pt-BR.
const string FormattingCulture = "en-US";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------------------------
// Configuration and options
// ---------------------------------------------------------------------------------------------
builder.Services.Configure<CompanyOptions>(builder.Configuration.GetSection(CompanyOptions.SectionName));
builder.Services.Configure<SeoOptions>(builder.Configuration.GetSection(SeoOptions.SectionName));
builder.Services.Configure<SiteLocalizationOptions>(builder.Configuration.GetSection(SiteLocalizationOptions.SectionName));

SiteLocalizationOptions siteLocalization =
    builder.Configuration.GetSection(SiteLocalizationOptions.SectionName).Get<SiteLocalizationOptions>()
    ?? new SiteLocalizationOptions();

// The connection string never has a default and never lives in a committed file (D24). Failing
// here, by name, is the difference between "the site cannot start" and "the site started against
// a database nobody meant to use".
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No connection string. Set it once for this clone with: " +
        "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<the value>\" " +
        "--project src/OrlandoUp.Web");
}

// ---------------------------------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Length beats composition rules: a long passphrase is both stronger and rememberable (D8/01).
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 1;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "OrlandoUp.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/admin/login";
    options.LogoutPath = "/admin/logout";
    options.AccessDeniedPath = "/admin/login";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
    options.AddPolicy(AuthorizationPolicies.Staff, policy => policy.RequireRole(Roles.Admin, Roles.Staff)));

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(FormattingCulture, siteLocalization.DefaultCulture);
    options.SupportedCultures = [new CultureInfo(FormattingCulture)];
    options.SupportedUICultures = siteLocalization.SupportedUICultures.Select(name => new CultureInfo(name)).ToList();
    options.ApplyCurrentCultureToResponseHeaders = true;

    // Two providers, in this order and no others. The URL decides for the public site (D21); the
    // cookie decides for the administration (D4/01). The header of the browser decides nothing:
    // a Brazilian browser asking for the English address must get the English page.
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CultureSegmentRequestCultureProvider(
        FormattingCulture, siteLocalization.DefaultCulture, SiteCultures.Portuguese)
    { Options = options });
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider { Options = options });
});

builder.Services.AddRazorPages(options =>
    {
        options.Conventions.Add(new CultureRouteConvention());
        options.Conventions.AuthorizeFolder("/Admin", AuthorizationPolicies.Staff);
        options.Conventions.AllowAnonymousToPage("/Admin/Login");
    })
    .AddViewLocalization();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<RichText>();
builder.Services.AddScoped<CatalogQueries>();

WebApplication app = builder.Build();

// ---------------------------------------------------------------------------------------------
// The two seeding commands run and exit without ever opening a port (D5/01).
// ---------------------------------------------------------------------------------------------
if (args.Length > 0 && SeedCommands.IsSeedCommand(args[0]))
{
    return await SeedCommands.RunAsync(app.Services, args[0]);
}

// ---------------------------------------------------------------------------------------------
// Pipeline. The order below is the whole point of this block, so it is written down:
//
//   1. exception and status-code pages first, so everything after them can be handled;
//   2. HSTS and the redirect to HTTPS outside Development;
//   3. static files, which never need routing, culture or a user;
//   4. UseRouting, which is what matches the request to a page and therefore what puts the
//      culture segment into the route values;
//   5. UseRequestLocalization AFTER routing, because the culture provider reads a route value
//      that does not exist before the route is matched, and BEFORE authorization, so that what
//      authorization returns already speaks the visitor's language;
//   6. authentication, then authorization;
//   7. the endpoints.
// ---------------------------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error/500");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
        context.Context.Response.Headers.CacheControl = "public,max-age=604800",
});

app.UseRouting();

app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHealthEndpoints();
app.MapRobotsTxt();

app.Run();

return 0;
