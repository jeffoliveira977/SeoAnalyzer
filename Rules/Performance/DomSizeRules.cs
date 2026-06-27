using AngleSharp.Dom;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Performance;

/// <summary>Audits total DOM size of the page.</summary>
internal static class DomSizeRules
{
    public static SeoAudit Execute(IDocument doc)
    {
        var totalElements = doc.All.Length;
        var passed = totalElements <= 1500;

        return new SeoAudit
        {
            Title = "DOM Size",
            Passed = passed,
            Value = $"The document has {totalElements} DOM elements.",
            Weight = 3,
            Recommendation = passed ? null : "Reduce the number of DOM elements. An excessive DOM size increases memory usage and delays page rendering.",
            Category = AuditCategory.Performance
        };
    }
}
