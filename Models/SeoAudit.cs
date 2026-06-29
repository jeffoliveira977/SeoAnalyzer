using System.Text.Json.Serialization;

namespace SeoAnalyzer.Models;

/// <summary>A single check item in the report.</summary>
public sealed class SeoAudit
{
    public required string Title { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AuditStatus Status { get; init; }

    /// <summary>Found value or text summary.</summary>
    public string? Value { get; init; }

    /// <summary>Fix suggestion when Status is not Passed.</summary>
    public string? Recommendation { get; init; }

    /// <summary>Structured details (images, headings, meta tags, etc.).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Details
    {
        get => _details;
        set => _details = value is System.Collections.ICollection { Count: 0 } ? null : value;
    }
    private object? _details;

    /// <summary>Category of the audit (SEO, Performance, or Security).</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AuditCategory Category { get; init; } = AuditCategory.Seo;
}
