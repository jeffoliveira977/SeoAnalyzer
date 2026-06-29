using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;
namespace SeoAnalyzer.Rules.Security;

/// <summary>Audits external target="_blank" links for rel="noopener" or rel="noreferrer" attributes to prevent tabnabbing.</summary>
internal static class ExternalLinksSecurityRules
{
    public static SeoAudit Execute(List<IHtmlAnchorElement> links)
    {

        var riskyLinks = links.Where(l =>
        {
            var href = l.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(href) ||
                !Uri.TryCreate(href, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }

            if (!string.Equals(l.Target, "_blank", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (l.Relation is string rel && !string.IsNullOrWhiteSpace(rel))
            {
                return !rel.Contains("noopener", StringComparison.OrdinalIgnoreCase) &&
                       !rel.Contains("noreferrer", StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }).ToList();

        var passed = riskyLinks.Count == 0;

        return new SeoAudit
        {
            Title = "External Links Security (noopener)",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? "All external target='_blank' links are secure." : $"{riskyLinks.Count} external link(s) are missing rel='noopener' or rel='noreferrer'.",
            Recommendation = passed
                ? null
                : "Use rel='noopener' or rel='noreferrer' on links that open in a new tab (target='_blank') to prevent reverse tabnabbing vulnerabilities.",
            Details = passed ? null : DomHelper.FormatAuditDetails(riskyLinks),
            Category = AuditCategory.Security
        };
    }
}
