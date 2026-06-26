using AngleSharp.Dom;
using System.Text.RegularExpressions;

namespace SeoAnalyzer;

/// <summary>Extracts and ranks keywords from main page content.</summary>
public static partial class CommonKeywordsRules
{
    [GeneratedRegex(@"\p{L}+", RegexOptions.Compiled)]
    private static partial Regex LettersOnlyRegex();

    [GeneratedRegex(@"\n{2,}", RegexOptions.Compiled)]
    private static partial Regex ParagraphSeparatorRegex();

    public static List<SeoAudit> Execute(IDocument document)
    {
        var audits = new List<SeoAudit>();
        AuditCommonKeywords(document, audits);
        return audits;
    }

    private static readonly string[] ChromeWords =
    [
        "nav", "menu", "header", "footer", "cookie", "banner", "popup","modal","advert", "ads", "social", 
        "newsletter", "sidebar", "comment", "pagination", "breadcrumb", "toolbar",  "widget", "carousel","slider"
    ];

    /// <summary>Skips UI chrome blocks (nav, footer, ads, etc.).</summary>
    private static bool IsChrome(IElement element)
    {
        var id = element.Id?.ToLowerInvariant() ?? string.Empty;
        var cls = element.ClassName?.ToLowerInvariant() ?? string.Empty;

        return ChromeWords.Any(word => id.Contains(word) || cls.Contains(word));
    }

    private static void AuditCommonKeywords(IDocument document, List<SeoAudit> audits)
    {
        var body = ExtractBodyText(document);
        var keywords = RankKeywords(body, 10);

        var hasKeywords = keywords.Length > 0;

        audits.Add(new SeoAudit
        {
            Title = "Common Keywords Presence",
            Passed = hasKeywords,
            Value = hasKeywords ? string.Join(", ", keywords) : "No common commercial keywords detected.",
            Weight = 6,
            Recommendation = hasKeywords
                ? null
                : "Consider adding relevant commercial keywords in title and first paragraph if applicable."
        });
    }
    private static string ExtractBodyText(IDocument document)
    {
        var content =
            document.QuerySelector("main") ??
            document.QuerySelector("article") ??
            document.Body;

        if (content == null)
            return string.Empty;

        var blocks = content.QuerySelectorAll("article, section, div")
                .Where(static e => e.QuerySelector("script,style,noscript") == null && !IsChrome(e))
                .Select(static e => e.TextContent.Trim())
                .Where(static t => t.Length > 300)
                .Where(static t => LettersOnlyRegex().Matches(t).Count >= 50)
                .OrderByDescending(ScoreBlock)
                .Take(3);

        return string.Join("\n\n", blocks);
    }

    private static int ScoreBlock(string text)
    {
        var words = LettersOnlyRegex().Matches(text).Count;
        var paragraphs = ParagraphSeparatorRegex().Matches(text).Count;

        return words + paragraphs * 120;
    }

    private static string[] RankKeywords(string text, int topN)
    {
        var normalized = TextHelper.RemoveDiacritics(text.ToLowerInvariant());

        return [..TextHelper.ExtractWords(normalized)
                  .GroupBy(w => w)
                  .OrderByDescending(g => g.Count())
                  .Take(topN)
                  .Select(g => g.Key)
               ];
    }
}