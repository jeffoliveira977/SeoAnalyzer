namespace SeoAnalyzer;

/// <summary>Social meta tag state (OG / Twitter).</summary>
public class MetaTagAuditItem
{
    public string Tag { get; set; } = string.Empty;
    public string? Value { get; set; }
    public bool Present { get; set; }
}
