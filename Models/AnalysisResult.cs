namespace SeoAnalyzer.Models;

/// <summary>Represents the final result of the page analysis.</summary>
public sealed class AnalysisResult
{
    /// <summary>Detailed metrics for SEO category.</summary>
    public CategorySummary? Seo { get; set; }

    /// <summary>Detailed metrics for Performance category.</summary>
    public CategorySummary? Performance { get; set; }

    /// <summary>Detailed metrics for Security category.</summary>
    public CategorySummary? Security { get; set; }

    /// <summary>Overall weighted score from 0 to 100.</summary>
    public int TotalScore { get; set; }
}