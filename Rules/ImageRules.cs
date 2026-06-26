using AngleSharp.Dom;
using System.Collections.Frozen;

namespace SeoAnalyzer;

/// <summary>Audits for alt text, dimensions, lazy loading and modern formats.</summary>
public static class ImageRules
{
    public static List<SeoAudit> Execute(IDocument doc)
    {
        var audits = new List<SeoAudit>();
        var images = doc.Images.OfType<IElement>().ToList();

        AuditAltTags(images, audits);
        AuditImageDimensions(images, audits);
        AuditLazyLoading(images, audits);
        AuditImageFormats(images, audits);

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
            Weight = 10,
            Recommendation = passed ? null : $"Use alt='' only for decorative images.",
            Details = passed ? null : BuildImageItems(missing)
        });
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
            Weight = 3,
            Recommendation = passed ? null : "Define width='' and height='' attributes to avoid layout shifts (CLS).",
            Details = passed ? null : BuildImageItems(missing)
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
            Weight = 3,
            Recommendation = passed ? null : "Use loading='lazy' for below-the-fold images to improve performance.",
            Details = passed ? null : BuildImageItems(missing)
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
            Weight = 4,
            Recommendation = passed ? null : "Serve images in WebP or AVIF whenever possible to reduce download size.",
            Details = passed ? null : BuildImageItems(missing)
        });

    }

    private static readonly string[] _snapshotAttrs = ["src", "alt", "width", "height", "loading"];

    /// <summary>Caps Details at 10 unique images per audit.</summary>
    private static List<ImageAuditItem> BuildImageItems(List<IElement> nodes)
    {
        const int MaxItems = 10;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<ImageAuditItem>();

        foreach (var img in nodes)
        {
            var src = img.GetAttribute("src");
            if (string.IsNullOrWhiteSpace(src) || !seen.Add(src)) continue;

            var attrs = _snapshotAttrs
                .Where(a => a != "src")
                .Select(a => (a, v: img.GetAttribute(a)))
                .Where(x => !string.IsNullOrWhiteSpace(x.v))
                .ToDictionary(x => x.a, x => x.v!);

            items.Add(new ImageAuditItem { Src = src });

            if (items.Count >= MaxItems) break;
        }

        return items;
    }
}
