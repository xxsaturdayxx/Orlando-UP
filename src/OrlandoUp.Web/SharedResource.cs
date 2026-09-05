using Microsoft.Extensions.Localization;

// The resource files are named after the ROOT NAMESPACE of the project, which is not the same as
// the assembly name here (OrlandoUp versus OrlandoUp.Web). Without this the localizer looks for a
// resource that does not exist, finds nothing, and quietly prints the KEY on the page instead of
// the text - which fails nothing and ships. The routing test asserts a real Portuguese word for
// exactly that reason.
[assembly: RootNamespace("OrlandoUp")]

namespace OrlandoUp;

/// <summary>
/// The marker type the string localizer is generic over. It lives in the root namespace and has no
/// members: the framework resolves Resources/SharedResource.resx from its name and namespace, and
/// a member here would only invite someone to put a string in code instead of in the resource file.
/// </summary>
public sealed class SharedResource
{
}
