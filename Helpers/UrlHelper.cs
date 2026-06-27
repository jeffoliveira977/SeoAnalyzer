using System.Collections.Frozen;
using System.Globalization;
using System.Net;

namespace SeoAnalyzer.Helpers;

/// <summary>Shared HTTP client for URL and robots.txt fetching.</summary>
internal static class UrlHelper
{
    internal static readonly HttpClient Http;

    private static readonly FrozenSet<string> _imageExtensions = FrozenSet.ToFrozenSet<string>([
     ".jpg", ".jpeg", ".png", ".gif", ".webp", ".avif",
         ".svg", ".ico", ".bmp", ".tiff", ".tif"
 ], StringComparer.OrdinalIgnoreCase);

    static UrlHelper()
    {
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            PooledConnectionLifetime = TimeSpan.FromMinutes(10)
        };

        Http = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        AddHeaders(Http);
    }

    public static void AddHeaders(HttpClient client)
    {
        var h = client.DefaultRequestHeaders;
        h.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36");
        h.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        h.TryAddWithoutValidation("Accept-Language", GetAcceptLanguage());
        h.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
        h.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
        h.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
        h.TryAddWithoutValidation("Sec-Fetch-Site", "none");
        h.TryAddWithoutValidation("Sec-Fetch-User", "?1");
        h.TryAddWithoutValidation("Cache-Control", "no-cache");
        h.TryAddWithoutValidation("Connection", "keep-alive");
        h.TryAddWithoutValidation("DNT", "1");
        h.TryAddWithoutValidation("Sec-Ch-Ua", "\"Chromium\";v=\"149\", \"Google Chrome\";v=\"149\", \"Not:A-Brand\";v=\"99\"");
        h.TryAddWithoutValidation("Sec-Ch-Ua-Mobile", "?0");
        h.TryAddWithoutValidation("Sec-Ch-Ua-Platform", "\"Windows\"");
    }

    private static string GetAcceptLanguage()
    {
        var culture = CultureInfo.CurrentUICulture;

        if (string.IsNullOrWhiteSpace(culture.Name))
            return "en-US,en;q=0.9";

        var language = culture.TwoLetterISOLanguageName;

        if (language.Equals("en", StringComparison.OrdinalIgnoreCase))
            return "en-US,en;q=0.9";

        return $"{culture.Name},{language};q=0.9,en-US;q=0.8,en;q=0.7";
    }


    /// <summary>Tries HEAD first; falls back to GET when needed.</summary>
    public static async Task<HttpResponseMessage?> FetchAsync(Uri uri)
    {
        try
        {
            var response = await Http.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, uri));

            if (response.IsSuccessStatusCode)
                return response;

            if (response.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotImplemented)
            {
                return await Http.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri),
                    HttpCompletionOption.ResponseHeadersRead);
            }

            return response;
        }
        catch
        {
            return null;
        }
    }

    public static string ResolveUrl(string href, string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return href;
        if (Uri.TryCreate(href, UriKind.Absolute, out _)) return href;

        try
        {
            return new Uri(new Uri(baseUrl), href).ToString();
        }
        catch
        {
            return href;
        }
    }

    public static string StripQuery(string url)
    {
        var q = url.IndexOf('?');
        if (q >= 0) url = url[..q];
        var h = url.IndexOf('#');
        if (h >= 0) url = url[..h];
        return url;
    }

    public static bool IsSameHost(string url, string host) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase);

    public static bool IsHttps(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase);

    public static bool IsImageUrl(string? src)
    {
        if (string.IsNullOrWhiteSpace(src)) return false;
        if (src.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) return false;

        var path = src.Contains('?') ? src[..src.IndexOf('?')] : src;
        var path2 = path.Contains('#') ? path[..path.IndexOf('#')] : path;

        var ext = Path.GetExtension(path2);
        return _imageExtensions.Contains(ext);
    }

    public static async Task<bool> ExistsAsync(Uri uri) =>
        (await FetchAsync(uri))?.IsSuccessStatusCode ?? false;
}
