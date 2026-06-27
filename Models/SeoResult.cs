using System.Collections.Generic;

namespace SeoAnalyzer.Models;

public sealed class SeoResult
{
    public int Score { get; set; }

    public List<SeoAudit> Audits { get; set; } = [];
}
