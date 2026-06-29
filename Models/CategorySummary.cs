using System.Collections.Generic;

namespace SeoAnalyzer.Models;

/// <summary>Encapsulates the aggregated metrics for a specific audit category.</summary>
public sealed class CategorySummary
{
    public int Score { get; set; }
    public int TotalPassed { get; set; }
    public int TotalFailed { get; set; }
    public int TotalWarnings { get; set; }

    public List<SeoAudit> Audits { get; set; } = [];
}