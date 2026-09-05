using Microsoft.AspNetCore.Identity;
using OrlandoUp.Application;
using OrlandoUp.Infrastructure.Data;

namespace OrlandoUp.Infrastructure.Seeding;

/// <summary>
/// The two commands the application answers on the command line instead of serving requests.
/// Both build the same services the site uses, run, and return an exit code: 0 done, 1 refused.
/// </summary>
public static class SeedCommands
{
    public const string Catalog = "seed-catalog";

    public const string Admin = "seed-admin";

    public static bool IsSeedCommand(string argument) =>
        argument is Catalog or Admin;

    public static async Task<int> RunAsync(IServiceProvider services, string command)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        ILoggerFactory factory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger logger = factory.CreateLogger("OrlandoUp.Seeding");

        if (command == Catalog)
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            IClock clock = scope.ServiceProvider.GetRequiredService<IClock>();

            return await CatalogSeeder.RunAsync(db, clock, logger, CancellationToken.None);
        }

        UserManager<IdentityUser> users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        RoleManager<IdentityRole> roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        return await AdminSeeder.RunAsync(users, roles, configuration, logger);
    }
}
