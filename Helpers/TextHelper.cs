using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SeoAnalyzer;

/// <summary>Text normalization and stopwords for keyword analysis.</summary>
internal static partial class TextHelper
{
    private static FrozenSet<string> _stopwords = [];

    static TextHelper()
    {
        LoadStopwords("en");
    }

    /// <summary>
    /// Loads stopwords for the given language + English base.
    /// </summary>
    public static void LoadStopwords(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang)) lang = "en";
        lang = lang.Trim().ToLowerInvariant();

        var json = TryReadEmbedded("stopwords-iso.json");
        if (json == null)
        {
            Console.Error.WriteLine($"[TextHelper] stopwords-iso.json not found.");
            return;
        }

        var combined = ParseStopwordsJson(json, "en");
        if (lang != "en")
            foreach (var w in ParseStopwordsJson(json, lang))
                combined.Add(w);

        if (combined.Count == 0)
        {
            Console.Error.WriteLine($"[TextHelper] No stopwords found for 'en+{lang}'.");
            return;
        }

        Interlocked.Exchange(ref _stopwords!, combined.ToFrozenSet(StringComparer.OrdinalIgnoreCase));
    }

    private static bool IsStopword(string word) =>
        _stopwords.Contains(word);

    private static HashSet<string> ParseStopwordsJson(string json, string lang)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return result;
            if (!doc.RootElement.TryGetProperty(lang, out var arr)
                || arr.ValueKind != JsonValueKind.Array)
                return result;

            foreach (var item in arr.EnumerateArray())
            {
                var w = item.GetString()?.Trim();
                if (!string.IsNullOrEmpty(w)) result.Add(w);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[TextHelper] Failed to parse stopwords for '{lang}': {ex.Message}");
        }
        return result;
    }

    private static string? TryReadEmbedded(string filename)
    {
        var asm = Assembly.GetExecutingAssembly();
        foreach (var name in asm.GetManifestResourceNames())
        {
            if (!name.EndsWith(filename, StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                using var stream = asm.GetManifestResourceStream(name)!;
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[TextHelper] Failed to read embedded '{name}': {ex.Message}");
            }
        }
        return null;
    }


    /// <summary>Strips diacritics for word comparison.</summary>
    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var ch in text.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return WhiteSpaceRegex().Replace(NonWordRegex().Replace(text, " "), " ").Trim();
    }

    /// <summary>Extracts valid words, filtering numbers, emails and stopwords.</summary>
    public static string[] ExtractWords(string text, int minimumLength = 4, bool removeStopwords = true)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        return [.. NormalizeText(text)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.ToLowerInvariant())
            .Where(w => w.Length >= minimumLength)
            .Where(w => !IsNumberRegex().IsMatch(w))
            .Where(w => !w.Contains('@'))
            .Where(w => !removeStopwords || !IsStopword(w))];
    }


    [GeneratedRegex(@"[^\p{L}\p{N}\s]+")]
    private static partial Regex NonWordRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpaceRegex();

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex IsNumberRegex();
}