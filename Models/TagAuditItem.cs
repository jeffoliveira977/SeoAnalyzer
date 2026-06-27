namespace SeoAnalyzer.Models;

/// <summary>Deprecated HTML tag found on the page.</summary>
public sealed class TagAuditItem
{
    public string Tag { get; init; } = default!;
    public string Fix { get; init; } = default!;
    public string[] Snippets { get; init; } = [];
}
