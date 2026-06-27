namespace SeoAnalyzer.Helpers;

/// <summary>Helper to identify WordPress core or plugin.</summary>
internal static class WordPressHelper
{

    public static bool IsWordPressCoreOrPlugin(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return url.Contains("/wp-includes/", StringComparison.OrdinalIgnoreCase) || 
               url.Contains("/wp-content/plugins/", StringComparison.OrdinalIgnoreCase);
    }
}
