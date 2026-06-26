# SeoAnalyzer

A lightweight, high-performance C# library designed for deep on-page SEO analysis. Built on top of `AngleSharp`, it fast-parses raw HTML or live URLs to deliver structured, production-ready diagnostic reports complete with automated audits, actionable fix recommendations, and a dynamic health score (0–100).

## Installation


```bash
dotnet add package SeoAnalyzer
```

## Use

```csharp
using SeoAnalyzer;

// From HTML string
var result = await SeoAnalyzer.FromHtmlAsync(html);

// From a URL
var result = await SeoAnalyzer.FromUrlAsync("https://example.com");

Console.WriteLine($"Score: {result.Score}/100");

foreach (var audit in result.Audits)
{
    Console.WriteLine($"[{(audit.Passed ? "Yes" : "No")}] {audit.Title}: {audit.Value}");
}
```
## Advanced: Customizing Rules
You can select exactly which rules you want to run using the AnalyzerOptions and SeoRules flags:

```csharp
using SeoAnalyzer;

// Run only Metadata and Images analysis
var options = new AnalyzerOptions
{
    Rules = SeoRules.Metadata | SeoRules.Images
};

// Or run everything EXCEPT Social Media tags
var options = new AnalyzerOptions
{
    Rules = SeoRules.All & ~SeoRules.Social
};

var result = await SeoAnalyzer.FromUrlAsync("https://example.com", options);
```

## Export as JSON

```csharp

var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = { new JsonStringEnumConverter() }
});
```
## What is Analyzed?

Metadata: Title, Description, Canonical Tag, Robots, Viewport, Charset, Lang Attribute, and Favicon.

Content & Structure: Single H1 Tag, Heading Hierarchy, and Common Keywords Presence.

Links: Internal vs External Links ratio, Empty Anchor Text, and External Links Security (noopener).

Images: Missing Alt Text, Missing Width/Height (CLS), Lazy Loading, and Modern Image Formats (WebP/AVIF).

Social & Structured Data: Open Graph (Facebook/LinkedIn), Twitter Cards, and JSON-LD Structured Data.

Technical SEO: Robots.txt, XML Sitemap, HTML Size, Deprecated HTML Tags/Attributes, and Google Tag Manager (Scripts, Noscripts, dataLayer).

## Response Structure

The analysis returns a `SeoAnalysis` object containing the overall score and a collection of individual audits.

### Data Models

#### SeoAnalysis
| Property | Type | Description |
| :--- | :--- | :--- |
| `Score` | `int` | Rating from 0 to 100 (weighted average of passed audits). |
| `Audits` | `List<SeoAudit>` | List of all executed SEO checks. |

#### SeoAudit
| Property | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string` | The name of the audit performed. |
| `Passed` | `bool` | `true` if the check met the SEO requirements, otherwise `false`. |
| `Value` | `string?` | The found value or a brief textual summary. |
| `Recommendation`| `string?` | Actionable fix suggestion (omitted in JSON if null). |
| `Weight` | `int` | The importance weight used to calculate the final score. |
| `Details` | `object?` | Structured diagnostic data (omitted in JSON if null). |

---

### Structured Details Types

When an audit fails or requires deeper diagnostics, the `Details` property will contain one of the following concrete types depending on the audit `Title`:

* **`HeadingAuditItem`**: Contains the hierarchy and count of heading tags (`H1` to `H6`).
* **`ImageAuditItem`**: Lists specific image URLs (`Src`) that failed checks (e.g., missing alt text or missing dimensions).
* **`MetaTagAuditItem`**: Provides a breakdown of specific social tags (`Open Graph` or `Twitter Cards`) detailing which ones are present or missing.
* **`TagAuditItem`**: Lists obsolete or deprecated HTML tags found within the document.
* **`AttributeAuditItem`**: Lists deprecated HTML attributes used in the document markup.

## Requirements
.NET 9.0 SDK or higher.
