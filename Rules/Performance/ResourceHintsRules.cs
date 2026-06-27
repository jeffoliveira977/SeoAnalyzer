using AngleSharp.Dom;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Performance;

/// <summary>Audits existence of resource pre-connections tags.</summary>
internal static class ResourceHintsRules
{
    public static SeoAudit Execute(System.Collections.Generic.IEnumerable<IElement> linkTags)
    {
        var hasHints = linkTags.Any(l => string.Equals(l.GetAttribute("rel"), "preconnect", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(l.GetAttribute("rel"), "dns-prefetch", StringComparison.OrdinalIgnoreCase));

        return new SeoAudit
        {
            Title = "Resource Hints (Preconnect/DNS-Prefetch)",
            Passed = hasHints,
            Value = hasHints ? "The page uses resource hints for external connections." : "No resource hints (preconnect or dns-prefetch) found.",
            Weight = 1,
            Recommendation = hasHints ? null : "Use <link rel='preconnect'> or <link rel='dns-prefetch'> to establish early connections to important third-party origins (e.g. fonts, CDNs).",
            Category = AuditCategory.Performance
        };
    }
}
