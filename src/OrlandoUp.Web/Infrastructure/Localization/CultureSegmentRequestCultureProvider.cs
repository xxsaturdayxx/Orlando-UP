using Microsoft.AspNetCore.Localization;

namespace OrlandoUp.Infrastructure.Localization;

/// <summary>
/// Decides the culture of a public request from the first URL segment, and stands aside for the
/// administration so that the cookie provider behind it can answer (D4/01).
/// </summary>
/// <remarks>
/// The formatting culture is not decided here and never changes: it is always en-US (D20), so a
/// decimal bound from a form meets a point and never a comma. Only the UI culture moves.
/// </remarks>
public sealed class CultureSegmentRequestCultureProvider : RequestCultureProvider
{
    private readonly string _defaultUICulture;
    private readonly string _portugueseUICulture;
    private readonly string _formattingCulture;

    public CultureSegmentRequestCultureProvider(
        string formattingCulture,
        string defaultUICulture,
        string portugueseUICulture)
    {
        _formattingCulture = formattingCulture;
        _defaultUICulture = defaultUICulture;
        _portugueseUICulture = portugueseUICulture;
    }

    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext.Request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase))
        {
            // Nothing to say about an administration address: the cookie provider decides.
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        string? segment = httpContext.GetRouteValue(CultureRouteConvention.RouteValueKey) as string;

        string uiCulture = string.Equals(segment, CultureRouteConvention.PortugueseSegment, StringComparison.OrdinalIgnoreCase)
            ? _portugueseUICulture
            : _defaultUICulture;

        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(_formattingCulture, uiCulture));
    }
}
