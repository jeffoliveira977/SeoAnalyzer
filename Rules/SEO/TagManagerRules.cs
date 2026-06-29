using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System.Text.RegularExpressions;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for Google Tag Manager installation and dataLayer.</summary>
internal static partial class TagManagerRules
{
    [GeneratedRegex(@"GTM-[A-Z0-9]+", RegexOptions.IgnoreCase)]
    private static partial Regex GtmContainerRegex();

    public static List<SeoAudit> Execute(IDocument doc, List<IHtmlScriptElement> scripts)
    {
        var audits = new List<SeoAudit>();

        AuditGtmScript(scripts, audits);
        AuditGtmNoScript(doc, audits);
        AuditDataLayer(scripts, audits);

        return audits;
    }

    private static void AuditGtmScript(List<IHtmlScriptElement> scripts, List<SeoAudit> audits)
    {
        var hasGtmScript = scripts.Any(script =>
            ContainsGtm(script.Source) ||
            ContainsGtm(script.Text));

        audits.Add(new SeoAudit
        {
            Title = "Google Tag Manager (script)",
            Status = hasGtmScript ? AuditStatus.Passed : AuditStatus.Warning,
            Value = hasGtmScript ? "GTM script present" : "Google Tag Manager (script) not found.",
            Recommendation = hasGtmScript
                ? null
                : "Add the Google Tag Manager <script> snippet into the <head> according to GTM documentation."
        });
    }

    private static void AuditGtmNoScript(IDocument doc, List<SeoAudit> audits)
    {
        var hasNoScript = doc.QuerySelectorAll("noscript iframe")
            .Any(iframe => ContainsGtm(iframe.GetAttribute("src")));

        audits.Add(new SeoAudit
        {
            Title = "Google Tag Manager (noscript)",
            Status = hasNoScript ? AuditStatus.Passed : AuditStatus.Warning,
            Value = hasNoScript ? "GTM <noscript> iframe present" : "GTM <noscript> snippet is missing.",
            Recommendation = hasNoScript
                ? null
                : "Add the GTM <noscript> snippet immediately after the opening <body> tag to support browsers without JavaScript."
        });
    }

    private static void AuditDataLayer(List<IHtmlScriptElement> scripts, List<SeoAudit> audits)
    {
        var hasDataLayer = scripts.Any(script => script.Text.Contains("dataLayer", StringComparison.OrdinalIgnoreCase));

        audits.Add(new SeoAudit
        {
            Title = "GTM dataLayer",
            Status = hasDataLayer ? AuditStatus.Passed : AuditStatus.Warning,
            Value = hasDataLayer ? "dataLayer present" : "No dataLayer detected.",
            Recommendation = hasDataLayer ? null : "Implement a dataLayer to send events and structured information to GTM."
        });
    }

    private static bool ContainsGtm(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Contains("gtm.js", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("ns.html", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("googletagmanager", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return GtmContainerRegex().IsMatch(value);
    }
}
