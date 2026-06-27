using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Models;
namespace SeoAnalyzer.Rules.Security;

/// <summary>Audits external target="_blank" links for rel="noopener" or rel="noreferrer" attributes to prevent tabnabbing.</summary>
internal static class ExternalLinksSecurityRules
{
    public static SeoAudit Execute(IDocument doc)
    {
        var links = doc.Links.OfType<IHtmlAnchorElement>().ToList();

        var riskyLinks = new List<string>();
        foreach (var link in links)
        {
            var href = link.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(href) ||
                !Uri.TryCreate(href, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                continue;
            }

            if (!string.Equals(link.Target, "_blank", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var rel = link.Relation ?? "";
            
            bool safe = rel.Contains("noopener", StringComparison.OrdinalIgnoreCase) ||
                        rel.Contains("noreferrer", StringComparison.OrdinalIgnoreCase);

            if (!safe)
            {
                riskyLinks.Add(href);
            }
        }

        var passed = riskyLinks.Count == 0;

        return new SeoAudit
        {
            Title = "External Links Security (noopener)",
            Passed = passed,
            Value = passed ? "All external target='_blank' links are secure." : $"{riskyLinks.Count} external link(s) are missing rel='noopener' or rel='noreferrer'.",
            Weight = 2,
            Recommendation = passed
                ? null
                : "Use rel='noopener' or rel='noreferrer' on links that open in a new tab (target='_blank') to prevent reverse tabnabbing vulnerabilities.",
            Details = passed ? null : riskyLinks.Distinct().ToList(),
            Category = AuditCategory.Security
        };
    }
}
