using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;
using System.Text.RegularExpressions;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for link counts and anchor text.</summary>
internal static class LinkRules
{
    public static List<SeoAudit> Execute(List<IHtmlAnchorElement> links, string url)
    {
        var audits = new List<SeoAudit>();

        AuditLinkCounts(url, links, audits);
        AuditEmptyAnchors(links, audits);

        return audits;
    }

    private static void AuditLinkCounts(string url, List<IHtmlAnchorElement> links, List<SeoAudit> audits)
    {

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
                href.StartsWith(url, StringComparison.OrdinalIgnoreCase) ||
                (!href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !href.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
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
            Status = internalCount > 0 ? AuditStatus.Passed : AuditStatus.Warning,
            Value = $"Internal: {internalCount}, External: {externalCount}",
            Recommendation = internalCount > 0
                ? null
                : "Ensure the page has internal links to help navigation and indexing."
        });
    }


    private static void AuditEmptyAnchors(List<IHtmlAnchorElement> links, List<SeoAudit> audits)
    {
        var emptyLinks = links
            .Where(link =>
            {
                if (!string.IsNullOrWhiteSpace(link.TextContent?.Trim()))
                    return false;

                if (!string.IsNullOrWhiteSpace(link.GetAttribute("aria-label")) ||
                    !string.IsNullOrWhiteSpace(link.GetAttribute("aria-labelledby")) ||
                    !string.IsNullOrWhiteSpace(link.GetAttribute("title")))
                    return false;

                var internalImage = link.QuerySelector("img");
                if (internalImage != null)
                {
                    if (!string.IsNullOrWhiteSpace(internalImage.GetAttribute("alt")))
                        return false;
                }

                return true;
            })
            .ToList();

        var emptyCount = emptyLinks.Count;
        var passed = emptyCount == 0;

        audits.Add(new SeoAudit
        {
            Title = "Empty Anchor Text",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? "All links have descriptive text." : $"{emptyCount} link(s) without descriptive text.",
            Recommendation = passed ? null : "Links without text, aria-label, aria-labelledby, title, or an image with an 'alt' attribute harm SEO and accessibility.",
            Details = DomHelper.FormatAuditDetails(emptyLinks)
        });
    }
}