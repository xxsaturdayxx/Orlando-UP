using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrlandoUp.Application;
using OrlandoUp.Infrastructure.Data;
using OrlandoUp.Infrastructure.Seeding;

namespace OrlandoUp.Tests;

/// <summary>
/// The site, running in this process, against a database that lives in memory.
/// </summary>
/// <remarks>
/// Two things here are deliberate and were decided in the review of the plan.
///
/// First, the application refuses to start when the connection string is missing, and that refusal
/// is right: it is what keeps a deployment from silently pointing at the wrong database. The test
/// host therefore satisfies the check by hand, with a value that is never dialled, and then throws
/// the provider away and puts SQLite in its place.
///
/// Second, the provider is SQLite and not SQL Server because the workflow runs on a Linux runner
/// where LocalDB does not exist. A suite that went red because of the machine it ran on would be a
/// suite people learn to ignore.
/// </remarks>
public sealed class SiteFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;
    private readonly List<string> _registeredServiceNames = [];

    /// <summary>
    /// The full name of every service type the application registered. Kept so a test can assert
    /// what is NOT there: a registration that never happens leaves no other trace.
    /// </summary>
    public IReadOnlyList<string> RegisteredServiceNames => _registeredServiceNames;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Never opened. It exists so the start-up check has something to find.
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=(test-host);Database=none");

        builder.ConfigureServices(services =>
        {
            // Removing the options type alone is not enough on this version of EF Core: the
            // provider chosen by the application arrives as an options CONFIGURATION registration,
            // and leaving it behind would put two providers on one context, which EF refuses.
            List<ServiceDescriptor> fromTheApplication = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(AppDbContext)
                    || descriptor.ServiceType.FullName?.Contains("DbContextOptions", StringComparison.Ordinal) == true)
                .ToList();

            foreach (ServiceDescriptor descriptor in fromTheApplication)
            {
                services.Remove(descriptor);
            }

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            _registeredServiceNames.Clear();
            _registeredServiceNames.AddRange(services.Select(descriptor =>
                descriptor.ServiceType.FullName ?? descriptor.ServiceType.Name));
        });
    }

    /// <summary>Creates the schema and writes the placeholder catalog into it.</summary>
    public async Task SeedAsync()
    {
        using IServiceScope scope = Services.CreateScope();

        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Creating the schema from the model, in a database that only exists for this test. The
        // application itself never does this: control C09 asserts as much over the source folder.
        await db.Database.EnsureCreatedAsync();

        IClock clock = scope.ServiceProvider.GetRequiredService<IClock>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Tests");

        await CatalogSeeder.RunAsync(db, clock, logger, CancellationToken.None);
    }

    /// <summary>Creates the schema and leaves it empty.</summary>
    public async Task CreateSchemaAsync()
    {
        using IServiceScope scope = Services.CreateScope();

        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
