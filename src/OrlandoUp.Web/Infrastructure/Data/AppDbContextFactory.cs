using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OrlandoUp.Infrastructure.Data;

/// <summary>
/// How the EF Core command-line tools build the context without starting the web application.
/// </summary>
/// <remarks>
/// It reads the connection string from user-secrets and from the environment — the two places a
/// secret is allowed to live (D24) — and never from a file in the repository. When the key is not
/// set it still builds a context without a connection string, which is enough to WRITE a migration
/// because writing one does not open a connection; applying a migration does, and that path needs
/// the key to be there.
/// </remarks>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(Program).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        DbContextOptionsBuilder<AppDbContext> options = new();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            options.UseSqlServer();
        }
        else
        {
            options.UseSqlServer(connectionString);
        }

        return new AppDbContext(options.Options);
    }
}
