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

        AuditSocialMetadata(doc, audits, "Open Graph", OgFields, "Add Open Graph tags (og:title, og:image) to improve social sharing.");
        AuditSocialMetadata(doc, audits, "Twitter Cards", TwitterFields, "Add Twitter Card meta tags to optimize presentation on Twitter.");

        return audits;
    }

    private static void AuditSocialMetadata(
        IDocument doc,
        List<SeoAudit> audits,
        string title,
        string[] fields,
        string recommendation)
    {
        var results = fields
            .Select(key => new
            {
               Name = key,
               Value = DomHelper.GetMetaContent(doc, key)   
            });

        var details = results
            .Where(r => !string.IsNullOrWhiteSpace(r.Value))
            .Select(r => $"{r.Name}: {r.Value}");

        var passed = details.Any();

        audits.Add(new SeoAudit
        {
            Title = title,
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Recommendation = passed ? null : recommendation,
            Details = passed ? details : null
        });
    }
}