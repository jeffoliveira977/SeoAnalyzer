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

        // Info audits are informational only — exclude them from the score denominator.
        var scorableAudits = categoryAudits.Where(a => a.Status != AuditStatus.Info).ToList();

        int totalPassed   = scorableAudits.Count(a => a.Status == AuditStatus.Passed);
        int totalFailed   = scorableAudits.Count(a => a.Status == AuditStatus.Failed);
        int totalWarnings = scorableAudits.Count(a => a.Status == AuditStatus.Warning);

        int score = scorableAudits.Count == 0
            ? 100
            : (int)Math.Round(((totalPassed * 1.0) + (totalWarnings * 0.5)) / scorableAudits.Count * 100);

        return new CategorySummary
        {
            Score = score,
            TotalPassed = totalPassed,
            TotalFailed = totalFailed,
            TotalWarnings = totalWarnings,
            Audits = categoryAudits   // return all audits (including Info) for display
        };
    }
}
