namespace SeoAnalyzer.Helpers;

/// <summary>
/// Parses and queries a raw HTTP cookie string (format: "name=value; name2=value2").
/// </summary>
internal static class CookieHelper
{
    /// <summary>
    /// Parses a raw cookie string into a dictionary of name → value pairs.
    /// Cookie names are compared case-insensitively.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Parse(string? cookieString)
    {
        if (string.IsNullOrWhiteSpace(cookieString))
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in cookieString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var eq = part.IndexOf('=');
            if (eq <= 0) continue;

            var name = part[..eq].Trim();
            var value = part[(eq + 1)..].Trim();

            if (!string.IsNullOrEmpty(name))
                result.TryAdd(name, value);
        }

        return result;
    }

    /// <summary>
    /// Returns <see langword="true"/> if any cookie name exactly matches or starts with
    /// any of the given <paramref name="signals"/> (case-insensitive).
    /// This supports prefix-based matching for cookies like <c>wordpress_logged_in_abc123</c>.
    /// </summary>
    public static bool HasCookieSignal(IReadOnlyDictionary<string, string> cookies, string[] signals)
    {
        if (cookies.Count == 0 || signals.Length == 0)
            return false;

        foreach (var name in cookies.Keys)
        {
            foreach (var signal in signals)
            {
                if (name.StartsWith(signal, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }
}
