using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Linq;

namespace SeoAnalyzer.Rules.Performance;

/// <summary>Audits for image dimensions, lazy loading and modern formats.</summary>
internal static class ImagePerformanceRules
{
    public static List<SeoAudit> Execute(List<IHtmlImageElement> images, string requestUrl)
    {
        var audits = new List<SeoAudit>();

        if (images == null || images.Count == 0)
            return audits;

        if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out var uri))
            return audits;

        var ownImages = images.Where(img => IsOwnDomain(img, uri.Host)).ToList();

        if (ownImages.Count == 0) return audits;

        AuditImageDimensions(ownImages, audits);
        AuditLazyLoading(ownImages, audits);
        AuditImageFormats(ownImages, audits);

        return audits;
    }

    private static void AuditImageDimensions(List<IHtmlImageElement> images, List<SeoAudit> audits)
    {
        var missing = images
            .Where(img => string.IsNullOrWhiteSpace(img.GetAttribute("width"))
                       || string.IsNullOrWhiteSpace(img.GetAttribute("height")))
            .ToList(); 

        var passed = missing.Count == 0;

        audits.Add(new SeoAudit
        {
            Title = "Image Width/Height",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? "All images have width and height defined."
                                    : $"There are {missing.Count} image(s) without width and height",
            Recommendation = passed ? null : "Define width='' and height='' attributes to avoid layout shifts (CLS).",
            Details = passed ? null : DomHelper.FormatAuditDetails(missing),
            Category = AuditCategory.Performance
        });
    }

    private static void AuditLazyLoading(List<IHtmlImageElement> images, List<SeoAudit> audits)
    {
        var firstImage = images.FirstOrDefault();

        var missing = images
            .Where(img => !string.Equals(img.GetAttribute("loading"), "lazy", StringComparison.OrdinalIgnoreCase))
            .Where(img => !string.Equals(img.GetAttribute("fetchpriority"), "high", StringComparison.OrdinalIgnoreCase))
            .Where(img => img != firstImage)
            .Where(img =>
            {
                var width = img.GetAttribute("width");
                var height = img.GetAttribute("height");
                if (int.TryParse(width, out var w) && w <= 200) return false;
                if (int.TryParse(height, out var h) && h <= 100) return false;
                return true;
            })
            .ToList();

        var passed = missing.Count == 0;

        audits.Add(new SeoAudit
        {
            Title = "Lazy Loading",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? "Lazy loading is correctly configured."
                                    : $"There are {missing.Count} image(s) without lazy loading",
            Recommendation = passed ? null : "Use loading='lazy' for below-the-fold images to improve performance.",
            Details = passed ? null : DomHelper.FormatAuditDetails(missing),
            Category = AuditCategory.Performance
        });
    }

    private static void AuditImageFormats(List<IHtmlImageElement> images, List<SeoAudit> audits)
    {
        var missing = images.Where(img => !ImageHelper.IsModernFormat(img)).ToList();
        var passed = missing.Count == 0;

        audits.Add(new SeoAudit
        {
            Title = "Modern Image Formats (WebP/AVIF)",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? "All images use modern formats."
                                    : $"{missing.Count} image(s) are not served in WebP or AVIF.",
            Recommendation = passed ? null : "Serve images in WebP or AVIF whenever possible to reduce download size.",
            Details = passed ? null : DomHelper.FormatAuditDetails(missing),
            Category = AuditCategory.Performance
        });
    }

    private static bool IsOwnDomain(IHtmlImageElement img, string host)
    {
        var src = img.GetAttribute("src");
        if (string.IsNullOrWhiteSpace(src)) return false;

        if (src.StartsWith("//"))
            return src[2..].StartsWith(host, StringComparison.OrdinalIgnoreCase);

        if (!src.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return true;

        return UrlHelper.IsSameHost(src, host);
    }
}