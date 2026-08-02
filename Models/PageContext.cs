using System.Net.Http.Headers;

namespace SeoAnalyzer.Models;

/// <summary>
/// Provides all the data needed to run a full audit.
/// Fill this from any source (Playwright, Selenium, Puppeteer, manual HTTP, etc.)
/// and pass it to <see cref="Seo.AnalyzeAsync(PageContext)"/>.
/// </summary>
public sealed record PageContext
{
    /// <summary>The raw HTML content of the page.</summary>
    public required string Html { get; init; }

    /// <summary>The final URL of the page (after redirects).</summary>
    public required string Url { get; init; }

    /// <summary>HTTP response headers (for security audits). Null to skip header-based checks.</summary>
    public HttpResponseHeaders? ResponseHeaders { get; init; }

    /// <summary>Network performance metrics (for performance audits). Null to skip network timing checks.</summary>
    public NetworkPerformanceMetrics? Metrics { get; init; }

    public string? Cookies { get; init; }
}
