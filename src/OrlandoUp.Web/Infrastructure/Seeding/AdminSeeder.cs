using Microsoft.AspNetCore.Identity;
using OrlandoUp.Application;

namespace OrlandoUp.Infrastructure.Seeding;

/// <summary>
/// Creates the two staff roles and the first administrator, from values that live only in
/// user-secrets or in the environment (D24). It refuses to run a second time.
/// </summary>
/// <remarks>
/// The refusal is not politeness: a command that could create a second administrator whenever it
/// was run would be a way to grant yourself access to a running site by way of a deploy script.
/// A password is never printed, never logged and never has a default.
/// </remarks>
internal static class AdminSeeder
{
    public const string EmailKey = "AdminSeed:Email";

    public const string PasswordKey = "AdminSeed:Password";

    public static async Task<int> RunAsync(
        UserManager<IdentityUser> users,
        RoleManager<IdentityRole> roles,
        IConfiguration configuration,
        ILogger logger)
    {
        foreach (string role in Roles.All)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                IdentityResult created = await roles.CreateAsync(new IdentityRole(role));

                if (!created.Succeeded)
                {
                    logger.LogError("seed-admin: could not create the role {Role}: {Errors}",
                        role, Describe(created));

                    return 1;
                }

                logger.LogInformation("seed-admin: created the role {Role}.", role);
            }
        }

        IList<IdentityUser> existing = await users.GetUsersInRoleAsync(Roles.Admin);

        if (existing.Count > 0)
        {
            logger.LogWarning(
                "seed-admin: an administrator already exists, so nothing was created. " +
                "Use the administration to add another one.");

            return 1;
        }

        string? email = configuration[EmailKey];
        string? password = configuration[PasswordKey];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogError(
                "seed-admin: {EmailKey} and {PasswordKey} are not set. Set them once for this clone " +
                "with dotnet user-secrets set, and never in a file of the repository.",
                EmailKey,
                PasswordKey);

            return 1;
        }

        IdentityUser user = new()
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        IdentityResult result = await users.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            logger.LogError("seed-admin: the account was refused: {Errors}", Describe(result));

            return 1;
        }

        IdentityResult inRole = await users.AddToRoleAsync(user, Roles.Admin);

        if (!inRole.Succeeded)
        {
            logger.LogError("seed-admin: the account was created but the role was refused: {Errors}",
                Describe(inRole));

            return 1;
        }

        logger.LogInformation("seed-admin: the first administrator was created.");

        return 0;
    }

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(error => error.Description));
}
