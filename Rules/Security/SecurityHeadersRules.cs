using System.Net.Http.Headers;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Security;

/// <summary>Audits response headers for security configuration.</summary>
internal static class SecurityHeadersRules
{
    public static SeoAudit? Execute(HttpResponseHeaders? responseHeaders)
    {
        if (responseHeaders == null)
            return null;

        var checks = new List<(string Name, bool Present, bool IsCritical, string Recommendation)>
        {
            ("Strict-Transport-Security", responseHeaders.Contains("Strict-Transport-Security"), true, "Enable HSTS to force connections over HTTPS."),
            ("X-Frame-Options", responseHeaders.Contains("X-Frame-Options"), false, "Set X-Frame-Options to protect against clickjacking attacks."),
            ("X-Content-Type-Options", responseHeaders.Contains("X-Content-Type-Options"), false, "Set X-Content-Type-Options to 'nosniff' to prevent MIME-type sniffing."),
            ("Referrer-Policy", responseHeaders.Contains("Referrer-Policy"), false, "Set Referrer-Policy to control the amount of referrer information sent.")
        };

        var missing = checks.Where(c => !c.Present).ToList();
        var passed = missing.Count == 0;

        var status = passed ? AuditStatus.Passed
             : missing.Any(c => c.IsCritical) ? AuditStatus.Failed
             : AuditStatus.Warning;

        return new SeoAudit
        {
            Title = "HTTP Security Headers",
            Status = status,
            Value = passed ? "All major HTTP security headers are present." : $"Missing {missing.Count} security header(s).",
            Recommendation = passed ? null : "Configure the missing HTTP security headers on your server response to protect client interactions.",
            Details = passed ? null : missing.Select(c => $"{c.Name}: {c.Recommendation}").ToList(),
            Category = AuditCategory.Security
        };
    }
}
