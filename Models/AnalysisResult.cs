namespace SeoAnalyzer.Models;

/// <summary>Represents the final result of the page analysis.</summary>
public sealed class AnalysisResult
{
    /// <summary>Overall weighted score from 0 to 100.</summary>
    public int Score { get; set; }

    /// <summary>SEO specific score (if executed).</summary>
    public int? SeoScore { get; set; }

    /// <summary>Performance specific score (if executed).</summary>
    public int? PerformanceScore { get; set; }

    /// <summary>Security specific score (if executed).</summary>
    public int? SecurityScore { get; set; }

    public List<SeoAudit> Audits { get; set; } = [];
}
