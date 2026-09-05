using System.Reflection;

namespace OrlandoUp.Tests;

/// <summary>
/// The first stage of the leva proves only that the two projects exist and that the test
/// project reaches the web assembly. Anchored on the assembly NAME, never on a type of it,
/// so it keeps meaning while the web project is still empty.
/// </summary>
public class SolutionWiringTests
{
    [Fact]
    public void The_web_assembly_is_reachable_from_the_test_project()
    {
        Assembly web = typeof(Program).Assembly;

        Assert.Equal("OrlandoUp.Web", web.GetName().Name);
    }
}
