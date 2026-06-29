using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for meta tags, title, canonical, charset, lang and favicon.</summary>
internal static class MetadataRules
{
    public static List<SeoAudit> Execute(IDocument doc, List<IHtmlLinkElement> headLinks)
    {
        var audits = new List<SeoAudit>();
        AuditTitle(doc, audits);
        AuditDescription(doc, audits);
        AuditCanonical(headLinks, audits);
        AuditRobots(doc, audits);
        AuditKeywords(doc, audits);
        AuditViewport(doc, audits);
        AuditCharset(doc, audits);
        AuditLang(doc, audits);
        AuditFavicon(headLinks, audits);

        return audits;
    }

    private static void AuditTitle(IDocument doc, List<SeoAudit> audits)
    {
        var title = DomHelper.GetTitle(doc)?.Trim() ?? string.Empty;

        AuditStatus titleStatus;
        string? titleRecommendation = null;

        if (string.IsNullOrWhiteSpace(title))
        {
            titleStatus = AuditStatus.Failed;
            titleRecommendation = "CRITICAL: The <title> tag is completely missing or empty. Search engines cannot index this page correctly without a title.";
        }
        else if (title.Length <= 15)
        {
            titleStatus = AuditStatus.Warning;
            titleRecommendation = "The page title is too short (should be above 15 characters). Expand it to describe the page content better and improve CTR.";
        }
        else
        {
            titleStatus = AuditStatus.Passed;
        }

        audits.Add(new SeoAudit
        {
            Title = "Page Title",
            Status = titleStatus,
            Value = string.IsNullOrWhiteSpace(title) ? "Page title missing." : title,
            Recommendation = titleRecommendation,
        });
    }

    private static void AuditDescription(IDocument doc, List<SeoAudit> audits)
    {
        var description = DomHelper.GetMetaContent(doc, "description") ?? string.Empty;
        var passed = description.Length > 20;

        audits.Add(new SeoAudit
        {
            Title = "Meta Description",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = string.IsNullOrWhiteSpace(description) ? "Meta description missing or poorly optimized." : description,
            Recommendation = passed ? null : "The meta description should be above 20 characters."
        });
    }

    private static void AuditCanonical(List<IHtmlLinkElement> headLinks, List<SeoAudit> audits)
    {
        var canonical = headLinks
            .FirstOrDefault(l => string.Equals(l.Relation, "canonical", StringComparison.OrdinalIgnoreCase))
            ?.Href;
        var passed = !string.IsNullOrWhiteSpace(canonical);

        audits.Add(new SeoAudit
        {
            Title = "Canonical Tag",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = canonical,
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
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = robots,
            Recommendation = passed ? null : "Consider adding a meta robots tag to instruct search engines."
        });
    }

    private static void AuditKeywords(IDocument doc, List<SeoAudit> audits)
    {
        var keywords = DomHelper.GetMetaContent(doc, "keywords");
        var passed = !string.IsNullOrWhiteSpace(keywords);

        audits.Add(new SeoAudit
        {
            Title = "Meta Keywords",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? keywords : "No meta keywords tag found.",
            Recommendation = passed ? null : "Although less relevant today, you can add meta keywords for internal organization."
        });
    }

    private static void AuditViewport(IDocument doc, List<SeoAudit> audits)
    {
        var viewport = DomHelper.GetMetaContent(doc, "viewport");
        var passed = !string.IsNullOrWhiteSpace(viewport);

        audits.Add(new SeoAudit
        {
            Title = "Viewport Tag",
            Status = passed ? AuditStatus.Passed : AuditStatus.Failed,
            Value = viewport,
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
            Status = passed ? AuditStatus.Passed : AuditStatus.Failed,
            Value = charset,
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
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = lang,
            Recommendation = passed ? null : "Set the 'lang' attribute on the <html> tag to help search engines and accessibility."
        });
    }

    private static void AuditFavicon(List<IHtmlLinkElement> headLinks, List<SeoAudit> audits)
    {
        var favicon = headLinks
            .FirstOrDefault(l =>
            {
                var rel = l.Relation;
                if (string.IsNullOrWhiteSpace(rel)) return false;
                var tokens = rel.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return tokens.Any(t => string.Equals(t, "icon", StringComparison.OrdinalIgnoreCase));
            })
            ?.Href;

        var passed = !string.IsNullOrWhiteSpace(favicon);

        audits.Add(new SeoAudit
        {
            Title = "Favicon",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = favicon,
            Recommendation = passed ? null : "A favicon helps brand recognition in search results."
        });
    }
}
