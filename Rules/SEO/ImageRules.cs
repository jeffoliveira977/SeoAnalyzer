using AngleSharp.Dom;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for image alt text tags.</summary>
internal static class ImageRules
{
    public static List<SeoAudit> Execute(List<IElement> images)
    {
        var audits = new List<SeoAudit>();
        AuditAltTags(images, audits);
        return audits;
    }

    private static void AuditAltTags(List<IElement> images, List<SeoAudit> audits)
    {
        var missing = images
            .Where(img => string.IsNullOrWhiteSpace(img.GetAttribute("alt")))
            .ToList();

        var passed = missing.Count == 0;

        audits.Add(new SeoAudit
        {
            Title = "Images Alt Text",
            Passed = passed,
            Value = images.Count == 0 ? "No images to analyze." : $"{images.Count} images are missing an alt attribute.",
            Weight = 3,
            Recommendation = passed ? null : "Use alt='' only for decorative images.",
            Details = passed ? null : BuildImageItems(missing)
        });
    }

    /// <summary>Caps Details at 10 unique images per audit.</summary>
    private static List<string> BuildImageItems(List<IElement> nodes)
    {
        const int MaxItems = 10;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<string>();

        foreach (var img in nodes)
        {
            var src = img.GetAttribute("src");
            if (string.IsNullOrWhiteSpace(src) || !seen.Add(src)) continue;

            items.Add(src);

            if (items.Count >= MaxItems) break;
        }

        return items;
    }
}
