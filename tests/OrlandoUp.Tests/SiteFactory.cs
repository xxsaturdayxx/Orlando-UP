using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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
    /// The clock this host runs on, frozen at the moment the factory was built. A test that cares
    /// what day it is moves it; every test that does not is unaffected, which is why it starts at
    /// the real instant rather than at some chosen date.
    /// </summary>
    public FakeClock Clock { get; } = new();

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

        // The test authentication seam of D1/04, and it is here — after the application has
        // registered everything — precisely so the application does not have to know it exists.
        builder.ConfigureTestServices(services =>
        {
            // Registered here rather than in ConfigureServices above, and the difference matters:
            // the snapshot of registered service names is taken before this runs, so the reflection
            // tests keep measuring what the APPLICATION registers and never see the test double.
            services.AddSingleton<IClock>(Clock);

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // Only the AUTHENTICATE scheme moves. The CHALLENGE scheme stays the Identity cookie
            // that AddIdentity chose, so a request without the header is still redirected to
            // /admin/login instead of getting a bare 401 — which is the behaviour
            // SiteBehaviourTests asserts, and it must keep asserting the application and not this
            // file. Registered last, so it is the last Configure to run and therefore the one
            // that wins.
            services.Configure<AuthenticationOptions>(options =>
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName);
        });
    }

    /// <summary>
    /// A client that is already signed in as a member of staff. The default
    /// <see cref="WebApplicationFactory{TEntryPoint}.CreateClient()"/> stays anonymous.
    /// </summary>
    public HttpClient CreateStaffClient(bool allowAutoRedirect = true)
    {
        HttpClient client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = allowAutoRedirect,
        });

        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, "1");

        return client;
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
