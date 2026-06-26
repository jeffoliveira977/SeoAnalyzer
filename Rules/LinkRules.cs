using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace SeoAnalyzer;

/// <summary>Audits for link counts, anchor text and external link security.</summary>
public static class LinkRules
{
    public static List<SeoAudit> Execute(IDocument doc)
    {
        var audits = new List<SeoAudit>();
        var links = doc.Links.OfType<IHtmlAnchorElement>().ToList();
        var baseUrl = DomHelper.ExtractBaseUrl(doc);

        AuditLinkCounts(baseUrl, links, audits);
        AuditEmptyAnchors(links, audits);
        AuditExternalLinksSecurity(links, audits);

        return audits;
    }

    private static void AuditLinkCounts(
        string? baseUrl,
        IReadOnlyList<IHtmlAnchorElement> links,
        List<SeoAudit> audits)
    {
        if (baseUrl == null)
        {
            audits.Add(new SeoAudit
            {
                Title = "Internal vs External Links",
                Passed = false,
                Weight = 5,
                Recommendation = "Add a canonical tag so internal links can be counted."
            });

            return;
        }

        var internalCount = 0;
        var externalCount = 0;

        foreach (var link in links)
        {
            var href = link.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(href) ||
                href.StartsWith('#') ||
                href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (href.StartsWith('/') ||
                href.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
            {
                internalCount++;
            }
            else if (Uri.TryCreate(href, UriKind.Absolute, out var uri) &&
                     (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                externalCount++;
            }
        }

        audits.Add(new SeoAudit
        {
            Title = "Internal vs External Links",
            Passed = internalCount > 0,
            Value = $"Internal: {internalCount}, External: {externalCount}",
            Weight = 5,
            Recommendation = internalCount > 0
                ? null
                : "Ensure the page has internal links to help navigation and indexing."
        });
    }

    private static void AuditEmptyAnchors(
        IReadOnlyList<IHtmlAnchorElement> links,
        List<SeoAudit> audits)
    {
        var emptyCount = links.Count(link =>
            string.IsNullOrWhiteSpace(link.TextContent.Trim()) &&
            string.IsNullOrWhiteSpace(link.GetAttribute("aria-label")));

        var passed = emptyCount == 0;

        audits.Add(new SeoAudit
        {
            Title = "Empty Anchor Text",
            Passed = passed,
            Value = emptyCount == 0 ? "All links have descriptive text." : $"{emptyCount} links without descriptive text.",
            Weight = 7,
            Recommendation = passed
                ? null
                : "Links without text or aria-label harm SEO and accessibility."
        });
    }

    private static void AuditExternalLinksSecurity(
        IReadOnlyList<IHtmlAnchorElement> links,
        List<SeoAudit> audits)
    {
        var riskyLinks = links.Count(link =>
        {
            var href = link.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(href) ||
                !Uri.TryCreate(href, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }

            if (!string.Equals(link.Target, "_blank", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var rel = link.Relation;

            if(string.IsNullOrWhiteSpace(rel)) return false;

            return !rel.Contains("noopener", StringComparison.OrdinalIgnoreCase) &&
                   !rel.Contains("noreferrer", StringComparison.OrdinalIgnoreCase);
        });

        audits.Add(new SeoAudit
        {
            Title = "External Links Security (noopener)",
            Passed = riskyLinks == 0,
            Value = riskyLinks == 0 ? "No internal links found." : $"{riskyLinks} external links missing rel='noopener'.",
            Weight = 4,
            Recommendation = riskyLinks == 0
                ? null
                : "Use rel='noopener noreferrer' on links that open in a new tab."
        });
    }
}