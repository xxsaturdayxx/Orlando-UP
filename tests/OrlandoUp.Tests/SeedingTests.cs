using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OrlandoUp.Application;
using OrlandoUp.Infrastructure.Data;
using OrlandoUp.Infrastructure.Seeding;

namespace OrlandoUp.Tests;

/// <summary>
/// The two seeding commands, and above all their refusals. A seeder that runs twice is how an
/// edited catalog silently goes back to the placeholder text, and a command that can always make
/// one more administrator is a way to grant yourself access to a running site.
/// </summary>
public class SeedingTests : IAsyncLifetime
{
    private readonly SiteFactory _factory = new();

    public async Task InitializeAsync() => await _factory.CreateSchemaAsync();

    public Task DisposeAsync()
    {
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task The_catalog_command_writes_the_whole_catalog_into_an_empty_database()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IClock clock = scope.ServiceProvider.GetRequiredService<IClock>();

        int code = await CatalogSeeder.RunAsync(db, clock, NullLogger.Instance, CancellationToken.None);

        Assert.Equal(0, code);
        Assert.Equal(7, await db.Products.CountAsync());
        Assert.Equal(14, await db.ProductTranslations.CountAsync());
        Assert.Equal(7, await db.Units.CountAsync());
        Assert.Equal(6, await db.AddOns.CountAsync());
        Assert.Equal(4, await db.DeliveryZones.CountAsync());
        Assert.Equal(10, await db.DeliveryLocations.CountAsync());
    }

    [Fact]
    public async Task The_catalog_command_run_twice_changes_nothing_and_still_succeeds()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IClock clock = scope.ServiceProvider.GetRequiredService<IClock>();

        await CatalogSeeder.RunAsync(db, clock, NullLogger.Instance, CancellationToken.None);
        int before = await db.Products.CountAsync();

        int code = await CatalogSeeder.RunAsync(db, clock, NullLogger.Instance, CancellationToken.None);

        Assert.Equal(0, code);
        Assert.Equal(before, await db.Products.CountAsync());
    }

    [Fact]
    public async Task Every_seeded_product_carries_both_cultures()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IClock clock = scope.ServiceProvider.GetRequiredService<IClock>();

        await CatalogSeeder.RunAsync(db, clock, NullLogger.Instance, CancellationToken.None);

        List<int> cultureCounts = await db.Products
            .Select(product => product.Translations.Count)
            .ToListAsync();

        Assert.All(cultureCounts, count => Assert.Equal(2, count));
    }

    [Fact]
    public async Task The_admin_command_creates_the_roles_and_the_first_administrator()
    {
        using IServiceScope scope = _factory.Services.CreateScope();

        int code = await AdminSeeder.RunAsync(
            scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(),
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
            ConfigurationWith("first@example.test", "a-long-enough-passphrase"),
            NullLogger.Instance);

        Assert.Equal(0, code);

        UserManager<IdentityUser> users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        Assert.Single(await users.GetUsersInRoleAsync(Roles.Admin));
    }

    [Fact]
    public async Task The_admin_command_refuses_once_an_administrator_exists_and_creates_nothing()
    {
        using IServiceScope scope = _factory.Services.CreateScope();

        UserManager<IdentityUser> users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        RoleManager<IdentityRole> roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await AdminSeeder.RunAsync(users, roles, ConfigurationWith("first@example.test", "a-long-enough-passphrase"), NullLogger.Instance);

        int before = await users.Users.CountAsync();

        int code = await AdminSeeder.RunAsync(
            users, roles, ConfigurationWith("second@example.test", "another-long-passphrase"), NullLogger.Instance);

        Assert.Equal(1, code);
        Assert.Equal(before, await users.Users.CountAsync());
    }

    [Fact]
    public async Task The_admin_command_refuses_when_the_secrets_are_not_set()
    {
        using IServiceScope scope = _factory.Services.CreateScope();

        UserManager<IdentityUser> users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        int code = await AdminSeeder.RunAsync(
            users,
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
            new ConfigurationBuilder().Build(),
            NullLogger.Instance);

        Assert.Equal(1, code);
        Assert.Equal(0, await users.Users.CountAsync());
    }

    private static IConfiguration ConfigurationWith(string email, string password) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [AdminSeeder.EmailKey] = email,
                [AdminSeeder.PasswordKey] = password,
            })
            .Build();
}
