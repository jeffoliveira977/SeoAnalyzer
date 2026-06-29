using AngleSharp.Dom;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Security;

/// <summary>Audits password input fields for secure delivery and submission endpoints.</summary>
internal static class SecurePasswordRules
{
    public static SeoAudit? Execute(IDocument doc, string? requestUrl)
    {
        var passwordInputs = doc.QuerySelectorAll("input[type='password']");
        if (passwordInputs.Length == 0)
            return null;

        bool isHttpsPage = false;
        if (!string.IsNullOrWhiteSpace(requestUrl))
        {
            try
            {
                var uri = new Uri(requestUrl);
                isHttpsPage = string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase);
            }
            catch { }
        }
        else
        {
            isHttpsPage = true;
        }

        var insecureFormActions = new List<string>();
        foreach (var input in passwordInputs)
        {
            var form = input.Ancestors<IElement>().FirstOrDefault(e => string.Equals(e.TagName, "form", StringComparison.OrdinalIgnoreCase));
            if (form != null)
            {
                var action = form.GetAttribute("action");
                if (action != null && action.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    insecureFormActions.Add(action);
                }
            }
        }

        bool passed = isHttpsPage && insecureFormActions.Count == 0;

        string value;
        string? recommendation = null;

        if (!isHttpsPage)
        {
            value = "Password fields are served over an insecure HTTP connection.";
            recommendation = "Serve password inputs and the landing page over HTTPS to protect credentials during transmission.";
        }
        else if (insecureFormActions.Count > 0)
        {
            value = $"Found {insecureFormActions.Count} password form(s) submitting to insecure http:// endpoints.";
            recommendation = "Ensure all login form actions submit to secure https:// endpoints to prevent credentials from being intercepted in transit.";
        }
        else
        {
            value = "All password fields are served and submitted securely.";
        }

        return new SeoAudit
        {
            Title = "Secure Password Forms",
            Status = passed ? AuditStatus.Passed : AuditStatus.Failed,
            Value = value,
            Recommendation = recommendation,
            Details = passed ? null : insecureFormActions.Distinct().ToList(),
            Category = AuditCategory.Security
        };
    }
}
