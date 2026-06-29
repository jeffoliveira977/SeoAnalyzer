using AngleSharp.Dom;
using SeoAnalyzer.Models;
namespace SeoAnalyzer.Rules.Security;

/// <summary>Audits Content Security Policy (CSP) declaration in document metadata.</summary>
internal static class CspRules
{
    public static SeoAudit Execute(IDocument doc)
    {
        var metaCsp = doc.QuerySelectorAll("meta")
            .FirstOrDefault(m => string.Equals(m.GetAttribute("http-equiv"), "Content-Security-Policy", StringComparison.OrdinalIgnoreCase));

        var passed = metaCsp != null;

        return new SeoAudit
        {
            Title = "Content Security Policy (CSP)",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? "Content Security Policy is defined in meta tags." : "No Content Security Policy found in meta tags.",
            Recommendation = passed ? null : "Define a Content Security Policy (CSP) in your meta tags or server response headers to mitigate XSS (Cross-Site Scripting) vulnerabilities.",
            Category = AuditCategory.Security
        };
    }
}
