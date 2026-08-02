using System.Net.Http.Headers;

namespace SeoAnalyzer.Models;

/// <summary>Represents the HTTP download payload and connection metrics results.</summary>
public sealed class NetworkFetchResult(string html, NetworkPerformanceMetrics metrics, HttpResponseHeaders responseHeaders, string finalUrl, string? cookies)
{
    public string Html { get; } = html;
    public NetworkPerformanceMetrics Metrics { get; } = metrics;
    public HttpResponseHeaders ResponseHeaders { get; } = responseHeaders;
    public string FinalUrl { get; } = finalUrl;

    public string? Cookies { get; } = cookies;
}
