using AngleSharp.Dom;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for JSON-LD and Schema.org microdata.</summary>
internal static class StructuredDataRules
{
    public static List<SeoAudit> Execute(IDocument doc)
    {
        var audits = new List<SeoAudit>();
        var scripts = doc.Scripts;

        var jsonLdCount = scripts.Count(s =>
            string.Equals(
                s.Type,
                "application/ld+json",
                StringComparison.OrdinalIgnoreCase));

        var microdataCount = doc.QuerySelectorAll("[itemscope]").Length;

        AuditJsonLd(jsonLdCount, audits);
        AuditMicrodata(microdataCount, jsonLdCount, audits);

        return audits;
    }

    private static void AuditJsonLd(int count, List<SeoAudit> audits)
    {
        var passed = count > 0;

        audits.Add(new SeoAudit
        {
            Title = "JSON-LD Structured Data",
            Passed = passed,
            Value = passed ? count.ToString() : null,
            Weight = 4,
            Recommendation = passed
                ? null
                : "Use JSON-LD to provide structured data and obtain Rich Snippets in Google."
        });
    }

    private static void AuditMicrodata(int count, int jsonLdCount, List<SeoAudit> audits)
    {
        var hasAnyStructuredData = count > 0 || jsonLdCount > 0;

        audits.Add(new SeoAudit
        {
            Title = "Microdata (Schema.org)",
            Passed = hasAnyStructuredData,
            Value = $"JSON-LD: {jsonLdCount}, Microdata: {count}",
            Weight = 1,
            Recommendation = "Microdata is an alternative to JSON-LD, but JSON-LD is recommended by Google."
        });
    }
}