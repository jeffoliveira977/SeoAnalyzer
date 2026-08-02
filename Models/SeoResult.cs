
namespace SeoAnalyzer.Models;

public sealed class SeoResult
{
    public int Score { get; set; }
    public int TotalPassed { get; set; }
    public int TotalFailed { get; set; }
    public int TotalWarnings { get; set; }

    public List<SeoAudit> Audits { get; set; } = [];

    /// <summary>Detected technology stack (CMS, frameworks, reCAPTCHA).</summary>
    public TechResult? Tech { get; set; }
}
