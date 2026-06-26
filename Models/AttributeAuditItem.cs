namespace SeoAnalyzer;

/// <summary>Deprecated HTML attribute found on the page.</summary>
public sealed class AttributeAuditItem
{
    public string Attribute { get; init; } = default!;
    public string Fix { get; init; } = default!;
    public string[] Snippets { get; init; } = [];
}
