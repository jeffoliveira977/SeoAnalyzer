using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeoAnalyzer.Rules.Security;

/// <summary>
/// Detects insecure resources loaded over HTTP on an HTTPS page.
/// Browsers block or warn about these, which breaks functionality and harms user trust.
/// </summary>
internal static class InsecureResources
{
    public static SeoAudit? Execute( 
        IEnumerable<IElement> scripts, IEnumerable<IHtmlAnchorElement> links, IEnumerable<IElement> images, IEnumerable<IElement> metas,
        string requestUrl)
    {
        if (!UrlHelper.IsHttps(requestUrl)) return null;

        var insecureMetas = metas
            .Select(e => new
            {
                Name = e.GetAttribute("property") ?? e.GetAttribute("name") ?? "unknown",
                Value = e.GetAttribute("content") ?? string.Empty
            })
            .Where(m => !string.IsNullOrEmpty(m.Value) && m.Value.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            .Select(m => $"{m.Name}: {m.Value}")
            .Distinct()
            .ToList();

        var scriptsInsecure = CollectInsecure(scripts, "src");
        var stylesInsecure = CollectInsecure(
            links.Where(l => string.Equals(l.GetAttribute("rel"), "stylesheet", StringComparison.OrdinalIgnoreCase)),
            "href");

        var imagesInsecure = CollectInsecure(images, "src");

        var insecure = new Dictionary<string, object>();

        if (scriptsInsecure.Count > 0) insecure.Add("Scripts", scriptsInsecure);
        if (stylesInsecure.Count > 0) insecure.Add("Styles", stylesInsecure);
        if (imagesInsecure.Count > 0) insecure.Add("Images", imagesInsecure);
        if (insecureMetas.Count > 0) insecure.Add("MetaTags", insecureMetas);

        var total = scriptsInsecure.Count + stylesInsecure.Count + imagesInsecure.Count + insecureMetas.Count;
        var passed = insecure.Count == 0;

        return new SeoAudit
        {
            Title = "Insecure Resources",
            Status = passed ? AuditStatus.Passed : AuditStatus.Failed,
            Value = passed
                        ? "All resources are loaded securely over HTTPS."
                        : $"{total} resource(s) are loaded or defined over HTTP on an HTTPS page.",
            Recommendation = passed ? null : "Update all resource and meta content URLs to use 'https://' to avoid browser security warnings and secure social sharing properties.",
            Details = passed ? null : insecure,
            Category = AuditCategory.Security
        };
    }


    private static List<string> CollectInsecure(IEnumerable<IElement> elements, string attribute) =>
        elements
            .Select(e => e.GetAttribute(attribute))
            .Where(src => src != null && src.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()!;
}
