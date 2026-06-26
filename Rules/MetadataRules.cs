using AngleSharp.Dom;

namespace SeoAnalyzer;

/// <summary>Audits for meta tags, title, canonical, charset, lang and favicon.</summary>
public static class MetadataRules
{
    public static List<SeoAudit> Execute(IDocument doc)
    {
        var audits = new List<SeoAudit>();
        AuditTitle(doc, audits);
        AuditDescription(doc, audits);
        AuditCanonical(doc, audits);
        AuditRobots(doc, audits);
        AuditKeywords(doc, audits);
        AuditViewport(doc, audits);
        AuditCharset(doc, audits);
        AuditLang(doc, audits);
        AuditFavicon(doc, audits);

        return audits;
    }

    private static void AuditTitle(IDocument doc, List<SeoAudit> audits)
    {
        var title = DomHelper.GetTitle(doc) ?? string.Empty;
        var passed = !string.IsNullOrWhiteSpace(title) && title.Length >= 30 && title.Length <= 60;

        audits.Add(new SeoAudit
        {
            Title = "Page Title",
            Passed = passed,
            Value = string.IsNullOrWhiteSpace(title) ? "Page title missing." : title,
            Weight = 8,
            Recommendation = passed ? null : "The title should be between 30 and 60 characters for better SERP display.",
        });
    }

    private static void AuditDescription(IDocument doc, List<SeoAudit> audits)
    {
        var description = DomHelper.GetMetaContent(doc, "description");
        var passed = !string.IsNullOrWhiteSpace(description) && description.Length >= 120 && description.Length <= 160;

        audits.Add(new SeoAudit
        {
            Title = "Meta Description",
            Passed = passed,
            Value = string.IsNullOrWhiteSpace(description) ? "Meta description missing or poorly optimized." : description,
            Weight = 8,
            Recommendation = passed ? null : "The meta description should be between 120 and 160 characters."
        });
    }

    private static void AuditCanonical(IDocument doc, List<SeoAudit> audits)
    {
        var canonical = doc.QuerySelector("link[rel='canonical']")?.GetAttribute("href");
        var passed = !string.IsNullOrWhiteSpace(canonical);

        audits.Add(new SeoAudit
        {
            Title = "Canonical Tag",
            Passed = passed,
            Value = canonical,
            Weight = 10,
            Recommendation = passed ? null : "The canonical tag is essential to avoid duplicate content."
        });
    }

    private static void AuditRobots(IDocument doc, List<SeoAudit> audits)
    {
        var robots = DomHelper.GetMetaContent(doc, "robots");
        var passed = !string.IsNullOrWhiteSpace(robots);

        audits.Add(new SeoAudit
        {
            Title = "Meta Robots",
            Passed = passed,
            Value = robots,
            Weight = 5,
            Recommendation = passed ? null : "Consider adding a meta robots tag to instruct search engines."
        });
    }

    private static void AuditKeywords(IDocument doc, List<SeoAudit> audits)
    {
        var keywords = DomHelper.GetMetaContent(doc, "keywords");
        audits.Add(new SeoAudit
        {
            Title = "Meta Keywords",
            Passed = true,
            Value = keywords,
            Weight = 1,
            Recommendation = "Although less relevant today, meta keywords can be used for internal organization."
        });
    }

    private static void AuditViewport(IDocument doc, List<SeoAudit> audits)
    {
        var viewport = DomHelper.GetMetaContent(doc, "viewport");
        var passed = !string.IsNullOrWhiteSpace(viewport);

        audits.Add(new SeoAudit
        {
            Title = "Viewport Tag",
            Passed = passed,
            Value = viewport,
            Weight = 5,
            Recommendation = passed ? null : "The viewport tag is crucial for mobile responsiveness."
        });
    }

    private static void AuditCharset(IDocument doc, List<SeoAudit> audits)
    {

        var charset = doc.QuerySelector("meta[charset]")?.GetAttribute("charset");

        var passed = !string.IsNullOrWhiteSpace(charset) && charset.Equals("utf-8", StringComparison.CurrentCultureIgnoreCase);

        audits.Add(new SeoAudit
        {
            Title = "Meta Charset",
            Passed = passed,
            Value = charset,
            Weight = 3,
            Recommendation = passed ? null : "Use <meta charset='UTF-8'> to ensure correct character rendering."
        });
    }

    private static void AuditLang(IDocument doc, List<SeoAudit> audits)
    {
        var lang = doc.QuerySelector("html")?.GetAttribute("lang");

        var passed = !string.IsNullOrWhiteSpace(lang);

        audits.Add(new SeoAudit
        {
            Title = "HTML Lang Attribute",
            Passed = passed,
            Value = lang,
            Weight = 5,
            Recommendation = passed ? null : "Set the 'lang' attribute on the <html> tag to help search engines and accessibility."
        });
    }

    private static void AuditFavicon(IDocument doc, List<SeoAudit> audits)
    {
        var favicon = doc.QuerySelectorAll("link")
         .FirstOrDefault(link =>
         link.GetAttribute("rel")?
         .Contains("icon", StringComparison.OrdinalIgnoreCase) == true)
         ?.GetAttribute("href");

        var passed = !string.IsNullOrWhiteSpace(favicon);

        audits.Add(new SeoAudit
        {
            Title = "Favicon",
            Passed = passed,
            Value = favicon,
            Weight = 2,
            Recommendation = passed ? null : "A favicon helps brand recognition in search results."
        });
    }
}
