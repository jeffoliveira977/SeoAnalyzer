namespace SeoAnalyzer;

/// <summary>Calculates score as weighted percentage of passed audits.</summary>
public static class ScoreCalculator
{
    public static int Calculate(SeoAnalysis analysis)
    {
        if (analysis.Audits.Count == 0) return 0;

        double total = analysis.Audits.Sum(a => a.Weight);
        if (total == 0) return 100;

        double passedWeight = analysis.Audits.Where(a => a.Passed).Sum(a => a.Weight);

        return (int)((passedWeight / total) * 100);
    }
}
