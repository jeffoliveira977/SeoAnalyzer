using SeoAnalyzer.Models;

namespace SeoAnalyzer.Helpers;

/// <summary>Calculates score as weighted percentage of passed audits.</summary>
internal static class ScoreCalculator
{
    public static int Calculate(IEnumerable<SeoAudit> audits, AuditCategory category)
    {
        var categoryAudits = audits.Where(a => a.Category == category).ToList();
        if (categoryAudits.Count == 0) return 100;

        double total = categoryAudits.Sum(a => a.Weight);
        if (total == 0) return 100;

        double passedWeight = categoryAudits.Where(a => a.Passed).Sum(a => a.Weight);

        return (int)((passedWeight / total) * 100);
    }
}
