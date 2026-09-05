using System.Xml.Linq;

namespace OrlandoUp.Tests;

/// <summary>
/// The two resource files are read from disk, not through the framework: a key present in one
/// culture and missing in the other is invisible at runtime, because the framework falls back to
/// printing the key itself, and a page shipping a bare key is how a translation gap reaches a
/// visitor without anything failing.
/// </summary>
public class LocalizationParityTests
{
    private static readonly string ResourcesFolder = LocateResources();

    [Fact]
    public void The_two_cultures_carry_exactly_the_same_keys()
    {
        HashSet<string> english = KeysOf("SharedResource.resx");
        HashSet<string> portuguese = KeysOf("SharedResource.pt-BR.resx");

        List<string> onlyEnglish = english.Except(portuguese).OrderBy(key => key).ToList();
        List<string> onlyPortuguese = portuguese.Except(english).OrderBy(key => key).ToList();

        Assert.True(
            onlyEnglish.Count == 0 && onlyPortuguese.Count == 0,
            $"only in en-US: {string.Join(", ", onlyEnglish)}; only in pt-BR: {string.Join(", ", onlyPortuguese)}");
    }

    [Theory]
    [InlineData("SharedResource.resx")]
    [InlineData("SharedResource.pt-BR.resx")]
    public void No_value_is_empty(string file)
    {
        List<string> empty = Entries(file)
            .Where(entry => string.IsNullOrWhiteSpace(entry.Value))
            .Select(entry => entry.Key)
            .ToList();

        Assert.True(empty.Count == 0, $"empty values in {file}: {string.Join(", ", empty)}");
    }

    [Fact]
    public void There_are_enough_keys_for_the_comparison_to_mean_something()
    {
        // A parity test over two empty files passes. This is the assertion that the universe being
        // compared is not empty, which is the same reason control C13 exists next to C12.
        Assert.True(KeysOf("SharedResource.resx").Count >= 20);
    }

    private static HashSet<string> KeysOf(string file) =>
        Entries(file).Select(entry => entry.Key).ToHashSet(StringComparer.Ordinal);

    private static List<KeyValuePair<string, string>> Entries(string file)
    {
        XDocument document = XDocument.Load(Path.Combine(ResourcesFolder, file));

        return document.Root!
            .Elements("data")
            .Select(element => new KeyValuePair<string, string>(
                element.Attribute("name")!.Value,
                element.Element("value")?.Value ?? string.Empty))
            .ToList();
    }

    private static string LocateResources()
    {
        DirectoryInfo? folder = new(AppContext.BaseDirectory);

        while (folder is not null)
        {
            string candidate = Path.Combine(folder.FullName, "src", "OrlandoUp.Web", "Resources");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            folder = folder.Parent;
        }

        throw new DirectoryNotFoundException("The Resources folder of the web project was not found.");
    }
}
