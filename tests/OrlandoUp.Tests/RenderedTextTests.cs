using System.Xml.Linq;

namespace OrlandoUp.Tests;

/// <summary>
/// No page may ship a resource KEY where its text belongs.
/// </summary>
/// <remarks>
/// This is here because it happened: the assembly name and the root namespace of the web project
/// differ, the localizer looked for a resource under the wrong name, found none, and did what it is
/// designed to do - print the key. Every page rendered, every status code was 200, and the site was
/// showing Rentals_Title as a heading. Nothing but reading the words could catch it.
/// </remarks>
public class RenderedTextTests : IAsyncLifetime
{
    private readonly SiteFactory _factory = new();

    public async Task InitializeAsync() => await _factory.SeedAsync();

    public Task DisposeAsync()
    {
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/pt")]
    [InlineData("/rentals")]
    [InlineData("/pt/rentals")]
    [InlineData("/rentals/standard-scooter")]
    [InlineData("/pt/rentals/standard-scooter")]
    [InlineData("/how-it-works")]
    [InlineData("/pt/how-it-works")]
    [InlineData("/faq")]
    [InlineData("/pt/faq")]
    [InlineData("/contact")]
    [InlineData("/privacy")]
    [InlineData("/terms")]
    [InlineData("/admin/login")]
    public async Task No_page_prints_a_resource_key(string path)
    {
        List<string> keys = ResourceKeys().ToList();

        // Reach first (rule 5): a body that contains none of zero keys proves nothing. If the resx
        // ever stops being found, parsed or filled, this fails here instead of turning fourteen
        // theory cases green for having scanned for nothing.
        Assert.True(
            keys.Count >= 20,
            $"Only {keys.Count} resource key(s) were read; the scan below would prove nothing.");

        string body = await _factory.CreateClient().GetStringAsync(path);

        List<string> leaked = keys
            .Where(key => body.Contains(key, StringComparison.Ordinal))
            .ToList();

        Assert.True(leaked.Count == 0, $"{path} printed the key(s): {string.Join(", ", leaked)}");
    }

    private static IEnumerable<string> ResourceKeys()
    {
        DirectoryInfo? folder = new(AppContext.BaseDirectory);

        while (folder is not null)
        {
            string candidate = Path.Combine(folder.FullName, "src", "OrlandoUp.Web", "Resources", "SharedResource.resx");

            if (File.Exists(candidate))
            {
                return XDocument.Load(candidate).Root!
                    .Elements("data")
                    .Select(element => element.Attribute("name")!.Value)
                    .ToList();
            }

            folder = folder.Parent;
        }

        throw new FileNotFoundException("The English resource file of the web project was not found.");
    }
}
