using System.Net.Http.Headers;

namespace SeoAnalyzer.Models;

/// <summary>Represents the HTTP download payload and connection metrics results.</summary>
public sealed class NetworkFetchResult(string html, NetworkPerformanceMetrics metrics, HttpResponseHeaders responseHeaders, string finalUrl, string? cookies)
{
    public string Html { get; } = html;
    public NetworkPerformanceMetrics Metrics { get; } = metrics;
    public HttpResponseHeaders ResponseHeaders { get; } = responseHeaders;
    public string FinalUrl { get; } = finalUrl;

    /// <summary>
    /// Cookies received from the server via <c>Set-Cookie</c> headers,
    /// serialized as a raw cookie string (e.g. "name=value; name2=value2").
    /// Null if the server set no cookies.
    /// </summary>
    public string? Cookies { get; } = cookies;
}
