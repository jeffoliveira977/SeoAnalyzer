using AngleSharp.Dom;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SeoAnalyzer.Rules.Performance;

/// <summary>Detects deprecated HTML tags and attributes.</summary>
internal class DeprecatedHtmlRules
{
    private static readonly Dictionary<string, string> DeprecatedTags = new()
    {
        ["center"] = "Use CSS: text-align: center; or Flexbox/Grid for layout.",
        ["font"] = "Use CSS: font-family, font-size, color instead of <font>.",
        ["marquee"] = "Use CSS animations or JavaScript for scrolling effects.",
        ["blink"] = "Avoid blinking text; if needed, use CSS animations (opacity/keyframes).",
        ["acronym"] = "Use <abbr> instead for both abbreviations and acronyms.",
        ["strike"] = "Use CSS: text-decoration: line-through or <s> for semantic strike.",
        ["s"] = "Prefer CSS text-decoration: line-through when possible.",
        ["big"] = "Use CSS font-size instead of <big>.",
        ["tt"] = "Use <code> or CSS font-family: monospace.",
        ["dir"] = "Replace with <ul> (unordered list) for directories/lists.",
        ["frameset"] = "Use modern layout (CSS Grid/Flexbox) instead of framesets.",
        ["frame"] = "Replace with <iframe> for embedding external content.",
        ["noframes"] = "Unnecessary in modern layout construction.",
        ["applet"] = "Use <object> or <embed> for external applications (Java applets deprecated).",
        ["basefont"] = "Use CSS global styles instead of <basefont>."
    };

    private static readonly Dictionary<string, string> DeprecatedAttributes = new()
    {
        ["align"] = "Use CSS: text-align, flexbox or grid alignment.",
        ["bgcolor"] = "Use CSS: background-color.",
        ["border"] = "Use CSS: border property.",
        ["color"] = "Use CSS: color property.",
        ["valign"] = "Use CSS: vertical-align.",
        ["hspace"] = "Use CSS: margin or padding instead.",
        ["vspace"] = "Use CSS: margin or padding instead.",
        ["frameborder"] = "Use CSS: border: none on iframe.",
        ["nowrap"] = "Use CSS: white-space: nowrap.",
    };

    public static List<SeoAudit> Execute(IDocument document)
    {
        var audits = new List<SeoAudit>();
        AuditDeprecatedHtmlTags(document, audits);
        AuditDeprecatedHtmlAttributes(document, audits);

        return audits;
    }

    private static void AuditDeprecatedHtmlTags(IDocument doc, List<SeoAudit> audits)
    {
        var results = DeprecatedTags
            .Select(kvp => new
            {
                Tag = kvp.Key,
                Fix = kvp.Value,
                Nodes = doc.QuerySelectorAll(kvp.Key)
            })
            .Where(x => x.Nodes.Length > 0)
            .ToList();

        var items = results.Select(r => new TagAuditItem
        {
            Tag = r.Tag,
            Fix = r.Fix,
            Snippets = [.. DomHelper.FormatAuditDetails(r.Nodes)]
        }).ToList();

        var totalIssues = results.Sum(r => r.Nodes.Length);
        var passed = totalIssues == 0;

        audits.Add(new SeoAudit
        {
            Title = "Deprecated HTML Tags",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? "No deprecated HTML tags found." : $"{totalIssues} issue(s) found",
            Recommendation = passed ? null : "Replace deprecated HTML tags with semantic HTML and modern CSS.",
            Details = passed ? null : items,
            Category = AuditCategory.Performance
        });
    }

    private static void AuditDeprecatedHtmlAttributes(IDocument document, List<SeoAudit> audits)
    {
        var results = DeprecatedAttributes
            .Select(kvp => new
            {
                Attribute = kvp.Key,
                Fix = kvp.Value,
                Nodes = document.QuerySelectorAll($"[{kvp.Key}]")
            })
            .Where(x => x.Nodes.Length > 0)
            .ToList();

        var items = results.Select(r => new AttributeAuditItem
        {
            Attribute = r.Attribute,
            Fix = r.Fix,
            Snippets = [.. DomHelper.FormatAuditDetails(r.Nodes)]
        }).ToList();

        var totalIssues = results.Sum(r => r.Nodes.Length);
        var passed = totalIssues == 0;

        audits.Add(new SeoAudit
        {
            Title = "Deprecated HTML Attributes",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = passed ? "No deprecated HTML attributes found." : $"{totalIssues} issue(s) found",
            Recommendation = passed ? null : "Replace deprecated HTML attributes with modern CSS equivalents.",
            Details = passed ? null : items,
            Category = AuditCategory.Performance
        });
    }

}