using AngleSharp.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for Open Graph and Twitter Cards.</summary>
internal static class SocialRules
{
    private static readonly string[] OgFields =
    [
        "og:title",
        "og:description",
        "og:image",
        "og:url",
        "og:type",
        "og:site_name",
        "og:locale"
    ];

    private static readonly string[] TwitterFields =
    [
        "twitter:card",
        "twitter:title",
        "twitter:description",
        "twitter:image",
        "twitter:site",
        "twitter:creator",
    ];

    public static List<SeoAudit> Execute(IDocument doc)
    {
        var audits = new List<SeoAudit>();
        AuditOpenGraph(doc, audits);
        AuditTwitterCards(doc, audits);
        return audits;
    }

    private static void AuditOpenGraph(IDocument doc, List<SeoAudit> audits)
    {
        var results = OgFields
            .Select(key =>
            {
                var value = DomHelper.GetMetaContent(doc, key);
                return new
                {
                    Tag = key,
                    Value = value,
                    Present = !string.IsNullOrWhiteSpace(value)
                };
            })
            .ToList();

        var passed = results.Any(r => r.Present);
        var details = results
            .Where(r => r.Present)
            .Select(r => new MetaTagAuditItem
            {
                Tag = r.Tag,
                Value = r.Value
            })
            .ToList();

        audits.Add(new SeoAudit
        {
            Title = "Open Graph",
            Passed = passed,
            Weight = 1,
            Recommendation = passed ? null : "Add Open Graph tags (og:title, og:image) to improve social sharing.",
            Details = details
        });
    }

    private static void AuditTwitterCards(IDocument doc, List<SeoAudit> audits)
    {
        var results = TwitterFields
            .Select(key =>
            {
                var value = DomHelper.GetMetaContent(doc, key);
                return new
                {
                    Tag = key,
                    Value = value,
                    Present = !string.IsNullOrWhiteSpace(value)
                };
            })
            .ToList();

        var passed = results.Any(r => r.Present);
        var details = results
            .Where(r => r.Present)
            .Select(r => new MetaTagAuditItem
            {
                Tag = r.Tag,
                Value = r.Value
            })
            .ToList();

        audits.Add(new SeoAudit
        {
            Title = "Twitter Cards",
            Passed = passed,
            Weight = 1,
            Recommendation = passed ? null : "Add Twitter Card meta tags to optimize presentation on Twitter.",
            Details = details
        });
    }
}
