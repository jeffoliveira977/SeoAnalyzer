namespace SeoAnalyzer;

/// <summary>Heading with incorrect hierarchy.</summary>
public sealed class HeadingAuditItem
{
    public required string Tag { get; init; }
    public required string Text { get; init; }
    public required string Audit { get; init; }
}
