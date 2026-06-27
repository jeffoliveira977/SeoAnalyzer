using AngleSharp.Dom;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for PWA manifest.</summary>
internal static class HtmlRules
{
    public static List<SeoAudit> Execute(List<IElement> links)
    {
        var audits = new List<SeoAudit>();
        AuditManifest(links, audits);
        return audits;
    }

    private static void AuditManifest(List<IElement> links, List<SeoAudit> audits)
    {
        var manifest = links
            .FirstOrDefault(l => string.Equals(l.GetAttribute("rel"), "manifest", StringComparison.OrdinalIgnoreCase))
            ?.GetAttribute("href");
        var passed = !string.IsNullOrWhiteSpace(manifest);

        audits.Add(new SeoAudit
        {
            Title = "Web App Manifest",
            Passed = passed,
            Value = manifest,
            Weight = 1,
            Recommendation = passed ? null : "A web app manifest enables PWA features and improves mobile experience."
        });
    }
}
