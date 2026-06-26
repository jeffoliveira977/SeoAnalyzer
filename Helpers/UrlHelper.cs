using System.Net;

namespace SeoAnalyzer;

/// <summary>Shared HTTP client for URL and robots.txt fetching.</summary>
internal static class UrlHelper
{
    internal static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

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

    public static async Task<bool> ExistsAsync(Uri uri) =>
        (await FetchAsync(uri))?.IsSuccessStatusCode ?? false;
}
