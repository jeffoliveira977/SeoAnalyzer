using AngleSharp.Dom;

namespace SeoAnalyzer.Helpers;

/// <summary>Common HTML DOM queries.</summary>
internal static class DomHelper
{
    public static string? GetAttribute(IDocument doc, string selector, string attribute)
    {
        return doc.QuerySelector(selector)?.GetAttribute(attribute);
    }

    /// <summary>Looks up meta by name, property, og: or twitter:.</summary>
    public static string? GetMetaContent(IDocument doc, string name)
    {
        return doc.QuerySelector($"meta[name='{name}']")?.GetAttribute("content")
            ?? doc.QuerySelector($"meta[property='{name}']")?.GetAttribute("content")
            ?? doc.QuerySelector($"meta[property='og:{name}']")?.GetAttribute("content")
            ?? doc.QuerySelector($"meta[name='twitter:{name}']")?.GetAttribute("content");
    }

    public static string? GetTitle(IDocument doc)
    {
        return doc.Title?.Trim();
    }

    /// <summary>Extracts scheme and host from canonical, base or og:url.</summary>
    public static string? ExtractBaseUrl(IDocument doc)
    {
        var url = doc.QuerySelector("link[rel='canonical']")
                   ?.GetAttribute("href")
            ?? doc.QuerySelector("base[href]")
                   ?.GetAttribute("href")
            ?? doc.QuerySelector("meta[property='og:url']")
                   ?.GetAttribute("content");

        return !string.IsNullOrWhiteSpace(url) ? new Uri(url).GetLeftPart(UriPartial.Authority) : null;
    }
}
