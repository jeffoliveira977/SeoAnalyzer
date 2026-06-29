using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for image alt text tags.</summary>
internal static class ImageAltRules
{
    public static List<SeoAudit> Execute(List<IHtmlImageElement> images)
    {
        var audits = new List<SeoAudit>();

        if (images == null || images.Count == 0)
            return audits;

        AuditAltTags(images, audits);
        return audits;
    }

    private static void AuditAltTags(List<IHtmlImageElement> images, List<SeoAudit> audits)
    {
        var missing = images
            .Where(img => string.IsNullOrWhiteSpace(img.GetAttribute("alt"))).ToList();

        var passed = missing.Count == 0;

        audits.Add(new SeoAudit
        {
            Title = "Images Alt Text",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? "All images have alt defined." : $"{missing.Count} image(s) are missing an alt attribute.",
            Recommendation = passed ? null : "Use alt='' only for decorative images.",
            Details = passed ? null : DomHelper.FormatAuditDetails(missing)
        });
    }
}
