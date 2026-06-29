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

    /// <summary>
    /// Groups raw elements by their HTML content, applies dynamic length truncation, 
    /// and consolidates duplicates by summing their occurrences.
    /// </summary>
    public static IEnumerable<string> FormatAuditDetails(IEnumerable<IElement> nodes)
    {
        return nodes
            .Select(n => (Node: n, Html: TextHelper.CleanHtml(n.OuterHtml)))
            .Where(x => !string.IsNullOrWhiteSpace(x.Html))
            .GroupBy(x => x.Html)
            .Select(g => (g.First().Node, Count: g.Count()))
            .Select(x => (Truncated: TruncateElement(x.Node, x.Count > 1), x.Count))
            .GroupBy(x => x.Truncated)
            .Select(g =>
            {
                var total = g.Sum(x => x.Count);
                return total > 1
                    ? $"{g.Key} <!-- Appears {total}x on the page -->"
                    : g.Key;
            });
    }


    /// <summary>
    /// Truncates long attribute values and inner content to fit within a target length.
    /// Duplicates get tighter limits to reduce JSON payload.
    /// </summary>
    public static string TruncateElement(IElement element, bool isDuplicate)
    {
        var clean = TextHelper.CleanHtml(element.OuterHtml);

        int maxLength = isDuplicate ? 140 : 180;
        int innerLimit = isDuplicate ? 30 : 60;

        if (clean.Length <= maxLength) return clean;

        var clone = (IElement)element.Clone();

        var innerClean = TextHelper.CleanHtml(clone.InnerHtml);
        if (innerClean.Length > innerLimit)
        {
            clone.InnerHtml = TextHelper.Ellipsize(innerClean, innerLimit);
            clean = TextHelper.CleanHtml(clone.OuterHtml);
            if (clean.Length <= maxLength) return clean;
        }

        var budget = maxLength;
        bool reduced;
        do
        {
            reduced = false;
            var longest = clone.Attributes
                .OrderByDescending(a => a.Value.Length)
                .FirstOrDefault(a => a.Value.Length > 10);

            if (longest == null) break;

            var newLimit = Math.Max(10, (int)(longest.Value.Length * 0.8));
            clone.SetAttribute(longest.Name, TextHelper.Ellipsize(longest.Value, newLimit));

            clean = TextHelper.CleanHtml(clone.OuterHtml);
            reduced = true;
        }
        while (clean.Length > budget && reduced);

        return clean;
    }

}
