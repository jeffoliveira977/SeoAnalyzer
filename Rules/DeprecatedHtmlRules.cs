using AngleSharp.Dom;
using System.Text;

namespace SeoAnalyzer;

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
            .Select(kvp =>
            {
                var nodes = doc.QuerySelectorAll(kvp.Key);

                return new
                {
                    Tag = kvp.Key,
                    Fix = kvp.Value,
                    Nodes = nodes
                };
            })
            .Where(x => x.Nodes.Length > 0)
            .ToList();

        var passed = results.Count == 0;

        var affectedItems = results.Select(r => new TagAuditItem
        {
            Tag = r.Tag,
            Fix = r.Fix,
            Snippets = [.. r.Nodes.Select(n => BuildSnippet(n))]
        }).ToList();

        audits.Add(new SeoAudit
        {
            Title = "Deprecated HTML Tags",
            Passed = passed,
            Value = passed ? "No deprecated HTML tags found." : string.Join(", ", results.Select(r => $"{r.Tag}({r.Nodes.Length})")),
            Weight = 4,
            Recommendation = passed
                ? null
                : "Replace deprecated HTML tags with semantic HTML and modern CSS.",
            Details = affectedItems
        });
    }

    private static void AuditDeprecatedHtmlAttributes(IDocument document, List<SeoAudit> audits)
    {
        var affectedItems = new List<AttributeAuditItem>();

        foreach (var el in document.QuerySelectorAll("*"))
        {
            foreach (var attr in DeprecatedAttributes)
            {
                if (!el.HasAttribute(attr.Key))
                    continue;

                var snippets = new List<string>
                    {
                        BuildSnippet(el)
                    };

                affectedItems.Add(new AttributeAuditItem
                {
                    Attribute = attr.Key,
                    Fix = attr.Value,
                    Snippets = [.. snippets]
                });
            }
        }

        var passed = affectedItems.Count == 0;

        audits.Add(new SeoAudit
        {
            Title = "Deprecated HTML Attributes",
            Passed = passed,
            Value = passed ? "No deprecated HTML attributes found." : $"{affectedItems.Count} issues found",
            Weight = 5,
            Recommendation = passed
                ? null
                : "Replace deprecated HTML attributes with modern CSS equivalents.",
            Details = affectedItems
        });
    }

    private static string BuildSnippet(IElement element)
    {
        var tag = element.TagName.ToLowerInvariant();

        var sb = new StringBuilder();

        sb.Append($"<{tag}");

        foreach (var attr in element.Attributes)
        {
            var name = attr.Name;
            var value = attr.Value;

            sb.Append($" {name}=\"{value}\"");
        }

        sb.Append($">...</{tag}>");

        return sb.ToString();
    }
}
