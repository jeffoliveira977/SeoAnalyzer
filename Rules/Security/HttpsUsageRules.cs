using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Security;

/// <summary>Audits whether the page is served over HTTPS.</summary>
internal static class HttpsUsageRules
{
    public static SeoAudit? Execute(string? requestUrl)
    {
        if (string.IsNullOrWhiteSpace(requestUrl))
            return null;

        try
        {
            var uri = new Uri(requestUrl);
            var passed = string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase);

            return new SeoAudit
            {
                Title = "HTTPS Usage",
                Passed = passed,
                Value = passed ? "The site uses HTTPS." : "The site does not use HTTPS.",
                Weight = 6,
                Recommendation = passed ? null : "Configure HTTPS for your website. HTTPS encrypts all communication, preventing interception and injection attacks.",
                Category = AuditCategory.Security
            };
        }
        catch
        {
            return null;
        }
    }
}
