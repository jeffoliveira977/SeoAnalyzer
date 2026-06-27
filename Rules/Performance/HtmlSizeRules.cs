using AngleSharp.Dom;
using System.Text;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Performance;

/// <summary>Audits total HTML document payload size.</summary>
internal static class HtmlSizeRules
{
    public static List<SeoAudit> Execute(IDocument doc)
    {
        var audits = new List<SeoAudit>();
        AuditHtmlSize(doc, audits);
        return audits;
    }

    private static void AuditHtmlSize(IDocument doc, List<SeoAudit> audits)
    {
        if (doc.DocumentElement == null)
            return;

        var html = doc.DocumentElement.OuterHtml;
        var sizeKb = Encoding.UTF8.GetByteCount(html) / 1024.0;
        var passed = sizeKb < 600;

        audits.Add(new SeoAudit
        {
            Title = "HTML Size",
            Passed = passed,
            Value = $"{sizeKb:F2} KB",
            Weight = 2,
            Recommendation = passed ? null : "The HTML is very large (over 600KB). Consider optimizing and removing excess markup.",
            Category = AuditCategory.Performance
        });
    }
}
