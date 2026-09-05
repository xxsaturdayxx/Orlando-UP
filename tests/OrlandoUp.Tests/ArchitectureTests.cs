using System.Reflection;
using System.Runtime.CompilerServices;
using NetArchTest.Rules;
using OrlandoUp.Application;

namespace OrlandoUp.Tests;

/// <summary>
/// The layering of the web project is a folder convention, and a convention nobody measures stops
/// being one. These tests are the measurement.
/// </summary>
public class ArchitectureTests
{
    private static readonly Assembly Web = typeof(RichText).Assembly;

    [Fact]
    public void The_domain_depends_on_no_other_layer()
    {
        TestResult result = Types.InAssembly(Web)
            .That().ResideInNamespace("OrlandoUp.Domain")
            .ShouldNot().HaveDependencyOnAny(
                "OrlandoUp.Application",
                "OrlandoUp.Infrastructure",
                "OrlandoUp.Pages",
                "OrlandoUp.Api")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void The_application_layer_knows_nothing_about_infrastructure_or_pages()
    {
        TestResult result = Types.InAssembly(Web)
            .That().ResideInNamespace("OrlandoUp.Application")
            .ShouldNot().HaveDependencyOnAny("OrlandoUp.Infrastructure", "OrlandoUp.Pages")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void The_two_rich_text_packages_enter_through_one_type_only()
    {
        List<string> carriers = Types.InAssembly(Web)
            .That().HaveDependencyOnAny("Markdig", "Ganss")
            .GetTypes()
            .Where(type => type.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
            .Select(type => type.FullName ?? type.Name ?? string.Empty)
            .OrderBy(name => name)
            .ToList();

        Assert.Equal(new List<string> { typeof(RichText).FullName! }, carriers);
    }

    private static string Describe(TestResult result) =>
        result.FailingTypeNames is null
            ? "no failing type was reported"
            : string.Join(", ", result.FailingTypeNames);
}
