using System.Text.Json.Serialization;

namespace SeoAnalyzer.Models;

/// <summary>A single check item in the report.</summary>
public sealed class SeoAudit
{
    public required string Title { get; init; }

    public required bool Passed { get; init; }

    /// <summary>Found value or text summary.</summary>
    public string? Value { get; init; }

    /// <summary>Fix suggestion when Passed is false.</summary>
    public string? Recommendation { get; init; }

    /// <summary>Weight used in the final score calculation.</summary>
    public int Weight { get; init; }

    /// <summary>Structured details (images, headings, meta tags, etc.).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Details { get; set; }

    /// <summary>Category of the audit (SEO or Performance).</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AuditCategory Category { get; init; } = AuditCategory.Seo;
}
