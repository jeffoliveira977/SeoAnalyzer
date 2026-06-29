using AngleSharp.Dom;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.SEO;

/// <summary>Audits for single H1 and heading hierarchy.</summary>
internal static class HeadingRules
{
    public static List<SeoAudit> Execute(IDocument doc)
    {
        var audits = new List<SeoAudit>();
        AuditH1(doc, audits);
        AuditHeadingHierarchy(doc, audits);
        return audits;
    }

    private static void AuditH1(IDocument doc, List<SeoAudit> audits)
    {
        var h1Elements = doc.QuerySelectorAll("h1");
        var count = h1Elements.Length;
        var passed = count == 1;

        audits.Add(new SeoAudit
        {
            Title = "Single H1 Tag",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = count == 0 ? "No H1 tag found." : count.ToString(),
            Recommendation = passed
                ? null
                : count == 0
                    ? "Add a single H1 describing the main topic of the page."
                    : "Use only one H1 as the primary heading."
        });
    }

    private static void AuditHeadingHierarchy(IDocument doc, List<SeoAudit> audits)
    {
        var headings = doc.QuerySelectorAll("h1,h2,h3,h4,h5,h6");

        int h2 = 0;
        int h3 = 0;

        var previousLevel = 0;

        var affectedItems = new List<HeadingAuditItem>();

        foreach (var heading in headings)
        {
            var level = heading.LocalName[1] - '0';

            switch (level)
            {
                case 2:
                    h2++;
                    break;
                case 3:
                    h3++;
                    break;
            }

            if (previousLevel == 0)
            {
                previousLevel = level;
                continue;
            }

            if (level > previousLevel + 1)
            {
                affectedItems.Add(new HeadingAuditItem
                {
                    Tag = heading.LocalName.ToUpperInvariant(),
                    Text = heading.TextContent.Trim(),
                    Audit = $"Expected H{previousLevel + 1}"
                });
            }

            previousLevel = level;

        }

        var passed = affectedItems.Count == 0 && (h2 > 0 || h3 > 0);

        audits.Add(new SeoAudit
        {
            Title = "Heading Hierarchy",
            Status = passed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = $"H2: {h2}, H3: {h3}",
            Recommendation = passed
                ? null
                : (h2 == 0 && h3 == 0)
                    ? "Add H2 and H3 headings to structure your content logically."
                    : "Use heading tags (H2–H6) in a logical hierarchy without skipping levels.",
            Details = (passed || affectedItems.Count == 0) ? null : affectedItems,
        });
    }
}