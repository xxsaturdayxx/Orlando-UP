using Microsoft.AspNetCore.Identity;
using OrlandoUp.Application;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Infrastructure.Seeding;

/// <summary>
/// The three commands the application answers on the command line instead of serving requests.
/// Each builds the same services the site uses, runs, and returns an exit code: 0 done, 1 refused.
/// </summary>
public static class SeedCommands
{
    public const string Catalog = "seed-catalog";

    public const string Admin = "seed-admin";

    public const string Batteries = "seed-batteries";

    public static bool IsSeedCommand(string argument) =>
        argument is Catalog or Admin or Batteries;

    public static async Task<int> RunAsync(IServiceProvider services, string command)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        ILoggerFactory factory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = factory.CreateLogger("OrlandoUp.Seeding");

        if (command == Catalog || command == Batteries)
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            IClock clock = scope.ServiceProvider.GetRequiredService<IClock>();

            return command == Catalog
                ? await CatalogSeeder.RunAsync(db, clock, logger, CancellationToken.None)
                : await BatterySeeder.RunAsync(db, clock, logger, CancellationToken.None);
        }

        UserManager<IdentityUser> users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        RoleManager<IdentityRole> roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        return await AdminSeeder.RunAsync(users, roles, configuration, logger);
    }
}
