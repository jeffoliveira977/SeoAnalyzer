namespace SeoAnalyzer;

/// <summary>Final SEO analysis result.</summary>
public sealed class SeoAnalysis
{
    /// <summary>Weighted score from 0 to 100.</summary>
    public int Score { get; set; }

    public List<SeoAudit> Audits { get; } = [];
}
