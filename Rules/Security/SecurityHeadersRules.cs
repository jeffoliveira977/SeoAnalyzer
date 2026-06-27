using System.Net.Http.Headers;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Security;

/// <summary>Audits response headers for security configuration.</summary>
internal static class SecurityHeadersRules
{
    public static SeoAudit? Execute(HttpResponseHeaders? responseHeaders)
    {
        if (responseHeaders == null)
            return null; // Skip if no headers are available (e.g. raw HTML analysis)

        var checks = new List<(string Name, bool Present, string Recommendation)>
        {
            ("Strict-Transport-Security",
            responseHeaders.Contains("Strict-Transport-Security"),
            "Enable HSTS to force connections over HTTPS."),
            ("X-Frame-Options",
            responseHeaders.Contains("X-Frame-Options"),
            "Set X-Frame-Options to protect against clickjacking attacks."),
            ("X-Content-Type-Options",
            responseHeaders.Contains("X-Content-Type-Options"),
            "Set X-Content-Type-Options to 'nosniff' to prevent MIME-type sniffing."),
            ("Referrer-Policy",
            responseHeaders.Contains("Referrer-Policy"),
            "Set Referrer-Policy to control the amount of referrer information sent.")
        };

        var missing = checks.Where(c => !c.Present).ToList();
        var passed = missing.Count == 0;

        return new SeoAudit
        {
            Title = "HTTP Security Headers",
            Passed = passed,
            Value = passed ? "All major HTTP security headers are present." : $"Missing {missing.Count} security header(s).",
            Weight = 4,
            Recommendation = passed ? null : "Configure the missing HTTP security headers on your server response to protect client interactions.",
            Details = passed ? null : missing.Select(c => $"{c.Name}: {c.Recommendation}").ToList(),
            Category = AuditCategory.Security
        };
    }
}
