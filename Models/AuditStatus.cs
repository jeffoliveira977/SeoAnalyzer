namespace SeoAnalyzer.Models;

public enum AuditStatus
{
    Passed,
    Failed,
    Warning,
    /// <summary>Informational only — does not affect the score.</summary>
    Info
}
