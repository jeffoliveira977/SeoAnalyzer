using AngleSharp.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for robots.txt, sitemap and noindex (requires base URL).</summary>
internal static class IndexingRules
{
    public static async Task<List<SeoAudit>> ExecuteAsync(IDocument doc, string requestUrl)
    {
        var audits = new List<SeoAudit>();

        var url = requestUrl.TrimEnd('/') + "/robots.txt";

        var content = await FetchRobotsTxtAsync(url);

        AuditRobotsTxt(url, content, audits);
        AuditSitemap(content, audits);
        AuditNoIndex(doc, audits);

        return audits;
    }

    private static async Task<string?> FetchRobotsTxtAsync(string url)
    {
        try
        {
            var response = await UrlHelper.Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return null;
        }
    }

    private static void AuditRobotsTxt(string url, string? content, List<SeoAudit> audits)
    {
        if (content == null)
        {
            audits.Add(new SeoAudit
            {
                Title = "Robots.txt Presence",
                Status = AuditStatus.Warning,
                Value = "Not Found",
                Recommendation = "Ensure robots.txt is accessible at the root of the domain."
            });
            return;
        }

        var blocksAll = content
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !l.StartsWith('#'))
            .Any(l => l.Equals("Disallow: /", StringComparison.OrdinalIgnoreCase));

        audits.Add(new SeoAudit
        {
            Title = "Robots.txt Presence",
            Status = blocksAll ? AuditStatus.Failed : AuditStatus.Passed,
            Value = url,
            Recommendation = blocksAll
                                 ? "robots.txt is blocking all crawlers (Disallow: /). Remove or restrict this rule."
                                 : null
        });
    }

    private static void AuditSitemap(string? content, List<SeoAudit> audits)
    {
        string? sitemapUrl = null;

        if (!string.IsNullOrWhiteSpace(content))
        {
            var lines = content.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in lines)
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
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = sitemapUrl ?? "No sitemap was declared in robots.txt.",
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
            Status = !isNoIndex ? AuditStatus.Passed : AuditStatus.Failed,
            Value = isNoIndex ? "NoIndex" : "Indexable",
            Recommendation = isNoIndex ? "The noindex tag is preventing this page from being indexed." : null
        });
    }
}
