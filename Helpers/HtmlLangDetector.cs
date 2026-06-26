using AngleSharp.Dom;

namespace SeoAnalyzer;

/// <summary>Normalizes the HTML lang attribute (e.g. "pt-BR" → "pt").</summary>
public static partial class HtmlLangDetector
{
    public static string Detect(IDocument document, string fallback = "en")
    {
        var lang = document.DocumentElement?.GetAttribute("lang");

        if (string.IsNullOrWhiteSpace(lang))
            return fallback;

        var separator = lang.IndexOfAny(['-', '_']);

        return separator >= 0
            ? lang[..separator].ToLowerInvariant()
            : lang.ToLowerInvariant();
    }
}
