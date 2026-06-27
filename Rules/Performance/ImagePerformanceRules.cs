using AngleSharp.Dom;
using SeoAnalyzer.Models;
using System.Collections.Frozen;

namespace SeoAnalyzer.Rules.Performance;

/// <summary>Audits for image dimensions, lazy loading and modern formats.</summary>
internal static class ImagePerformanceRules
{
    public static List<SeoAudit> Execute(List<IElement> images)
    {
        var audits = new List<SeoAudit>();

        AuditImageDimensions(images, audits);
        AuditLazyLoading(images, audits);
        AuditImageFormats(images, audits);

        return audits;
    }

    private static void AuditImageDimensions(List<IElement> images, List<SeoAudit> audits)
    {
        var missing = images
            .Where(img => string.IsNullOrWhiteSpace(img.GetAttribute("width"))
                       || string.IsNullOrWhiteSpace(img.GetAttribute("height")))
            .ToList();

        var passed = missing.Count == 0;

        audits.Add(new SeoAudit
        {
            Title = "Image Width/Height",
            Passed = passed,
            Value = images.Count == 0 ? "No images to analyze." : $"There are {missing.Count} images without width and height",
            Weight = 2,
            Recommendation = passed ? null : "Define width='' and height='' attributes to avoid layout shifts (CLS).",
            Details = passed ? null : BuildImageItems(missing),
            Category = AuditCategory.Performance
        });
    }

    private static void AuditLazyLoading(List<IElement> images, List<SeoAudit> audits)
    {
        var missing = images
            .Where(img => !string.Equals(img.GetAttribute("loading"), "lazy", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var passed = images.Count == 0 || missing.Count == 0;

        audits.Add(new SeoAudit
        {
            Title = "Lazy Loading",
            Passed = passed,
            Value = images.Count == 0 ? "No images to analyze." : $"There are {missing.Count} images without lazy loading",
            Weight = 2,
            Recommendation = passed ? null : "Use loading='lazy' for below-the-fold images to improve performance.",
            Details = passed ? null : BuildImageItems(missing),
            Category = AuditCategory.Performance
        });
    }

    private static readonly FrozenSet<string> _modernExtensions =
        FrozenSet.ToFrozenSet<string>([".webp", ".avif"]);

    private static bool IsModernFormat(IElement img)
    {
        var src = img.GetAttribute("src");
        if (string.IsNullOrWhiteSpace(src)) return false;
        var path = src.Contains('?') ? src[..src.IndexOf('?')] : src;
        return _modernExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static void AuditImageFormats(List<IElement> images, List<SeoAudit> audits)
    {
        var missing = images.Where(img => !IsModernFormat(img)).ToList();
        var passed = images.Count == 0 || missing.Count == 0;

        audits.Add(new SeoAudit
        {
            Title = "Modern Image Formats (WebP/AVIF)",
            Passed = passed,
            Value = images.Count == 0 ? "No images to analyze." : $"{missing.Count} images are not served in WebP or AVIF.",
            Weight = 2,
            Recommendation = passed ? null : "Serve images in WebP or AVIF whenever possible to reduce download size.",
            Details = passed ? null : BuildImageItems(missing),
            Category = AuditCategory.Performance
        });
    }

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
