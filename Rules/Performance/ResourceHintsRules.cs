using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Performance;

/// <summary>Audits existence of resource pre-connection hints for external origins.</summary>
internal static class ResourceHintsRules
{
    public static SeoAudit Execute(IEnumerable<IElement> scripts, List<IHtmlLinkElement> headLinks, string url)
    {
        var pageOrigin = UrlHelper.ExtractOrigin(url);

        var stylesheets = headLinks.Where(l => string.Equals(l.Relation, "stylesheet", StringComparison.OrdinalIgnoreCase));

        var allAssets = scripts.Cast<IElement>()
            .Concat(stylesheets.Cast<IElement>())
            .ToList();

        var hintedOrigins = headLinks
            .Where(l => !string.IsNullOrWhiteSpace(l.Href) && (string.Equals(l.Relation, "preconnect", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(l.Relation, "dns-prefetch", StringComparison.OrdinalIgnoreCase)))
            .Select(l => UrlHelper.ExtractOrigin(l.Href!))
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .ToList();

        var missingHints = allAssets
            .Where(asset =>
            {
                var path = string.Equals(asset.NodeName, "script", StringComparison.OrdinalIgnoreCase)
                    ? asset.GetAttribute("src")
                    : asset.GetAttribute("href");

                if (string.IsNullOrWhiteSpace(path)) return false;

                var origin = UrlHelper.ExtractOrigin(path);

                if (string.IsNullOrWhiteSpace(origin) || string.Equals(origin, pageOrigin, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (hintedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                    return false;

                return true;
            })
            .ToList();

        var passed = missingHints.Count == 0;

        return new SeoAudit
        {
            Title = "Resource Hints (Preconnect/DNS-Prefetch)",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed
                ? "All external origins have resource hints."
                : $"{missingHints.Count} external asset(s) without preconnect or dns-prefetch.",
            Recommendation = passed
                ? null
                : "Use <link rel='preconnect'> or <link rel='dns-prefetch'> to establish early connections to important third-party origins (e.g., CDNs, Google Fonts, APIs).",
            Details = passed ? null : DomHelper.FormatAuditDetails(missingHints),
            Category = AuditCategory.Performance
        };
    }
}
