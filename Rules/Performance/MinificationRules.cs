using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;
namespace SeoAnalyzer.Rules.Performance;

/// <summary>Audits whether external CSS and JS resources are minified using URL naming and content heuristics.</summary>
internal static class MinificationRules
{
    public static async Task<IEnumerable<SeoAudit>> ExecuteAsync(List<IHtmlScriptElement> scripts, List<IHtmlLinkElement> headLinks, string? requestUrl)
    {
        var stylesheets = headLinks
            .Where(l => string.Equals(l.Relation, "stylesheet", StringComparison.OrdinalIgnoreCase));

        var unminifiedCss = await GetUnminifiedAsync(stylesheets, l => l.Href, requestUrl);
        var unminifiedJs = await GetUnminifiedAsync(scripts, s => s.Source, requestUrl);

        return
        [
            BuildAudit("Minified CSS", unminifiedCss, "css", requestUrl),
            BuildAudit("Minified JS",  unminifiedJs,  "js", requestUrl),
        ];
    }

    private static SeoAudit BuildAudit(string title, List<string> unminified, string type, string? requestUrl)
    {
        var host = Uri.TryCreate(requestUrl, UriKind.Absolute, out var u) ? u.Host : string.Empty;

        var ownSite = unminified.Where(url => UrlHelper.IsSameHost(url, host)).ToList();
        var wordpress = unminified.Where(url => !UrlHelper.IsSameHost(url, host) && WordPressHelper.IsWordPressCoreOrPlugin(url)).ToList();

        var actionable = ownSite.Count + wordpress.Count;
        var passed = actionable == 0;

        var recommendation = BuildRecommendation(ownSite, wordpress, type);

        return new SeoAudit
        {
            Title = title,
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? $"All {type.ToUpperInvariant()} resources are minified."
                           : $"{actionable} {type.ToUpperInvariant()} resource(s) are not minified.",
            Recommendation = passed ? null : recommendation,
            Details = passed ? null : ownSite.Concat(wordpress).ToList(),
            Category = AuditCategory.Performance
        };
    }

    private static string BuildRecommendation(List<string> ownSite, List<string> wordpress, string type)
    {
        var parts = new List<string>();

        if (ownSite.Count > 0)
            parts.Add($"Minify your own {type.ToUpperInvariant()} files directly (use .min.{type.ToLowerInvariant()}).");

        if (wordpress.Count > 0)
            parts.Add("For WordPress plugin/theme files, use a minification plugin (e.g., Autoptimize, WP Rocket).");

        return string.Join(" ", parts);
    }

    private static async Task<List<string>> GetUnminifiedAsync<T>(
    IEnumerable<T> elements, Func<T, string?> urlSelector, string? requestUrl)
    {
        using var gate = new SemaphoreSlim(6, 6);

        var tasks = elements
            .Select(urlSelector)
            .Where(raw => !string.IsNullOrWhiteSpace(raw))
            .Select(async raw =>
            {
                await gate.WaitAsync();
                try
                {
                    var resolved = UrlHelper.ResolveUrl(raw!, requestUrl);
                    var minified = await IsMinifiedAsync(resolved);
                    return (raw: raw!, minified);
                }
                finally
                {
                    gate.Release();
                }
            });

        var results = await Task.WhenAll(tasks);
        return [.. results.Where(r => !r.minified).Select(r => r.raw)];
    }

    private static async Task<bool> IsMinifiedAsync(string url)
    {
        var clean = UrlHelper.StripQuery(url);

        if (clean.EndsWith(".min.css", StringComparison.OrdinalIgnoreCase) ||
            clean.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase))
            return true;

        var content = await FetchBeginningAsync(url);
        return content == null || IsContentMinified(content);
    }

    private static async Task<string?> FetchBeginningAsync(string url)
    {
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var response = await UrlHelper.Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            if (!response.IsSuccessStatusCode) return null;

            using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new StreamReader(stream);

            var buffer = new char[3000];
            var read = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
            return new string(buffer, 0, read);
        }
        catch { return null; }
    }

    private static bool IsContentMinified(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length < 150) return true;

        var whitespaceRatio = (double)content.Count(char.IsWhiteSpace) / content.Length;
        var lines = content.Split('\n');
        var avgLineLength = lines.Length > 0 ? (double)content.Length / lines.Length : content.Length;

        return whitespaceRatio < 0.15 || avgLineLength > 150;
    }
}