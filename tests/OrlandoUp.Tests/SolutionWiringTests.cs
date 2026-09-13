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

/// <summary>
/// What the application registers, read off the container it actually built.
/// </summary>
/// <remarks>
/// Anchored on service NAMES and never on types. Asking the container for a type the test can
/// already see compiles is a tautology; asking it for a name is the only form that would notice a
/// registration quietly removed.
/// </remarks>
public class BookingWiringTests : IDisposable
{
    private readonly SiteFactory _factory = new();

    public BookingWiringTests() => _factory.CreateClient().Dispose();

    public void Dispose() => _factory.Dispose();

    [Theory]
    [InlineData("AvailabilityQueries")]
    [InlineData("QuoteBuilder")]
    [InlineData("BookingWriter")]
    [InlineData("BookingTimeline")]
    public void Every_booking_service_is_registered(string name)
    {
        Assert.Contains(_factory.RegisteredServiceNames, registered => registered.EndsWith("." + name, StringComparison.Ordinal));
    }

    [Fact]
    public void The_registration_list_is_read_from_the_application_and_not_from_the_test_host()
    {
        // The presence half of the theory above: the list is non-trivial and carries the services
        // the earlier levas registered, so a match in it means something.
        Assert.Contains(_factory.RegisteredServiceNames, name => name.EndsWith(".CatalogQueries", StringComparison.Ordinal));

        // And the absence: the fake clock is registered in ConfigureTestServices, AFTER the
        // snapshot is taken, so it must not appear. If it did, this list would be describing the
        // harness rather than the application.
        Assert.DoesNotContain(_factory.RegisteredServiceNames, name => name.EndsWith(".FakeClock", StringComparison.Ordinal));
    }

    [Fact]
    public void This_leva_registers_no_e_mail_sender_and_no_payment_client()
    {
        // The neutralization step of a front with external effects, written as its presence half
        // while there are none: nothing here sends anything, and the day the payment front does,
        // this assertion is what will have to change deliberately.
        Assert.DoesNotContain(_factory.RegisteredServiceNames, name =>
            name.Contains("EmailSender", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Stripe", StringComparison.OrdinalIgnoreCase));
    }
}
