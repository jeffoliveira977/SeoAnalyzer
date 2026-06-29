using System;
using System.Collections.Generic;
using System.Linq;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Helpers;

/// <summary>Calculates score as percentage of passed/warned audits without weights.</summary>
internal static class ScoreCalculator
{
    public static CategorySummary? BuildSummary(List<SeoAudit> audits, AuditCategory category)
    {
        var categoryAudits = audits.Where(a => a.Category == category).ToList();
        if (categoryAudits.Count == 0) return null;

        int totalPassed = categoryAudits.Count(a => a.Status == AuditStatus.Passed);
        int totalFailed = categoryAudits.Count(a => a.Status == AuditStatus.Failed);
        int totalWarnings = categoryAudits.Count(a => a.Status == AuditStatus.Warning);

        double achieved = (totalPassed * 1.0)
                        + (totalWarnings * 0.5);

        int score = (int)Math.Round((achieved / categoryAudits.Count) * 100);

        return new CategorySummary
        {
            Score = score,
            TotalPassed = totalPassed,
            TotalFailed = totalFailed,
            TotalWarnings = totalWarnings,
            Audits = categoryAudits
        };
    }
}
