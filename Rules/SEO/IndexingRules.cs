using AngleSharp.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for robots.txt, sitemap and noindex (requires base URL).</summary>
internal static class IndexingRules
{
    private record RobotsTxtResult(bool Exists, string? Content, string? Url);

    public static async Task<List<SeoAudit>> ExecuteAsync(IDocument doc)
    {
        var audits = new List<SeoAudit>();
        var baseUrl = DomHelper.ExtractBaseUrl(doc);
        var robots = await FetchRobotsTxtAsync(baseUrl);

        AuditRobotsTxt(baseUrl, robots, audits);
        AuditSitemap(robots, audits);
        AuditNoIndex(doc, audits);

        return audits;
    }

    private static async Task<RobotsTxtResult> FetchRobotsTxtAsync(string? baseUrl)
    {
        if (baseUrl == null) return new(Exists: false, Content: null, Url: null);

        var url = baseUrl.TrimEnd('/') + "/robots.txt";
        try
        {
            var response = await UrlHelper.Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new(Exists: false, Content: null, Url: url);

            var content = await response.Content.ReadAsStringAsync();
            return new(Exists: true, Content: content, Url: url);
        }
        catch
        {
            return new(Exists: false, Content: null, Url: url);
        }
    }

    private static void AuditRobotsTxt(string? baseUrl, RobotsTxtResult robots, List<SeoAudit> audits)
    {
        if (baseUrl == null)
        {
            audits.Add(new SeoAudit
            {
                Title = "Robots.txt Presence",
                Passed = false,
                Weight = 3,
                Recommendation = "Could not determine base URL — add a canonical tag to enable robots.txt check."
            });
            return;
        }

        audits.Add(new SeoAudit
        {
            Title = "Robots.txt Presence",
            Passed = robots.Exists,
            Value = robots.Url,
            Weight = 3,
            Recommendation = robots.Exists ? null : "Ensure robots.txt is accessible at the root of the domain."
        });
    }

    private static void AuditSitemap(RobotsTxtResult robots, List<SeoAudit> audits)
    {
        string? sitemapUrl = null;

        if (robots.Content != null)
        {
            foreach (var raw in robots.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var line = raw.Trim();
                if (line.StartsWith('#')) continue;
                if (!line.StartsWith("Sitemap:", StringComparison.OrdinalIgnoreCase)) continue;

                sitemapUrl = line["Sitemap:".Length..].Trim();
                break;
            }
        }

        var passed = !string.IsNullOrWhiteSpace(sitemapUrl);

        audits.Add(new SeoAudit
        {
            Title = "Sitemap XML",
            Passed = passed,
            Value = sitemapUrl ?? "No sitemap was declared in robots.txt.",
            Weight = 3,
            Recommendation = passed ? null : "An XML sitemap helps search engines discover your pages."
        });
    }

    private static void AuditNoIndex(IDocument doc, List<SeoAudit> audits)
    {
        var robots = DomHelper.GetMetaContent(doc, "robots")
                  ?? DomHelper.GetMetaContent(doc, "googlebot");

        var isNoIndex = !string.IsNullOrWhiteSpace(robots)
                     && robots.Contains("noindex", StringComparison.OrdinalIgnoreCase);

        audits.Add(new SeoAudit
        {
            Title = "NoIndex",
            Passed = !isNoIndex,
            Value = isNoIndex ? "NoIndex" : "Indexable",
            Weight = 5,
            Recommendation = isNoIndex ? "The noindex tag is preventing this page from being indexed." : null
        });
    }
}
