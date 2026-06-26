using AngleSharp.Dom;
using System.Text;

namespace SeoAnalyzer;

/// <summary>Audits for PWA manifest and HTML size.</summary>
public static class HtmlRules
{
    public static List<SeoAudit> Execute(IDocument doc)
    {
        var audits = new List<SeoAudit>();
        AuditManifest(doc, audits);
        AuditHtmlSize(doc, audits);
        return audits;
    }

    private static void AuditManifest(IDocument doc, List<SeoAudit> audits)
    {
        var manifest = doc.QuerySelector("link[rel='manifest']")?.GetAttribute("href");
        var passed = !string.IsNullOrWhiteSpace(manifest);

        audits.Add(new SeoAudit
        {
            Title = "Web App Manifest",
            Passed = passed,
            Value = manifest,
            Weight = 2,
            Recommendation = passed ? null : "A web app manifest enables PWA features and improves mobile experience."
        });
    }

    private static void AuditHtmlSize(IDocument doc, List<SeoAudit> audits)
    {
       
        if (doc.DocumentElement == null)
            return;

        var html = doc.DocumentElement.OuterHtml;
        var sizeKb = Encoding.UTF8.GetByteCount(html) / 1024.0;
        var passed = sizeKb < 100;

        audits.Add(new SeoAudit
        {
            Title = "HTML Size",
            Passed = passed,
            Value = $"{sizeKb:F2} KB",
            Weight = 3,
            Recommendation = passed ? null : "The HTML is very large (over 100KB). Consider optimizing and removing excess markup."
        });
    }
}
