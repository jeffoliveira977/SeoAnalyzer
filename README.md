# SeoAnalyzer

A lightweight, high-performance C# library designed for deep on-page **SEO**, **Performance**, and **Security** analysis. Built on top of `AngleSharp`, it fast-parses raw HTML or live URLs to deliver structured, production-ready diagnostic reports complete with automated audits, actionable fix recommendations, and segmented scores.

## Installation

```bash
dotnet add package SeoAnalyzer
```

## Usage

The library has a clean, segmented API:
1. **`Seo.FromHtmlAsync(html)`**: Audits **SEO** specific metrics on raw HTML, returning a `SeoResult`.
2. **`Seo.FromUrlAsync(url)`**: Audits **SEO**, **Performance**, and **Security** metrics on a live webpage, returning a unified `AnalysisResult`.

### 1. SEO Analysis from HTML String

```csharp
using SeoAnalyzer;

// From HTML string
var result = await Seo.FromHtmlAsync(html);

if (result != null)
{
    Console.WriteLine($"SEO Score: {result.Score}/100");
    foreach (var audit in result.Audits)
    {
        Console.WriteLine($"[{audit.Category}] [{(audit.Passed ? "Yes" : "No")}] {audit.Title}: {audit.Value}");
    }
}
```

### 2. Full URL Analysis (SEO, Performance & Security)

```csharp
using SeoAnalyzer;

// From a live URL
var result = await Seo.FromUrlAsync("https://example.com");

if (result != null)
{
    Console.WriteLine($"Overall Score: {result.Score}/100");
    Console.WriteLine($"SEO Score: {result.SeoScore}/100");
    Console.WriteLine($"Performance Score: {result.PerformanceScore}/100");
    Console.WriteLine($"Security Score: {result.SecurityScore}/100");

    foreach (var audit in result.Audits)
    {
        Console.WriteLine($"[{audit.Category}] [{(audit.Passed ? "Yes" : "No")}] {audit.Title}: {audit.Value}");
    }
}
```

---

## What is Analyzed?

### 1. Search Engine Optimization (SEO) — *`Seo.FromHtmlAsync` and `Seo.FromUrlAsync`*
*   **Metadata:** Title, Description, Canonical Tag, Robots, Viewport, Charset, Lang Attribute, and Favicon.
*   **Content & Structure:** Single H1 Tag, Heading Hierarchy (fails if no H2 or H3 tags are present), and Common Keywords Presence.
*   **Links:** Internal vs External Links ratio and Empty Anchor Text.
*   **Images:** Missing Alt Text (details return a list of image URLs).
*   **Social & Structured Data:** Open Graph (Facebook/LinkedIn), Twitter Cards, and JSON-LD/Microdata Structured Data.
*   **Technical SEO:** Robots.txt, XML Sitemap, and Google Tag Manager (Scripts, Noscripts, dataLayer).

### 2. Performance — *`Seo.FromUrlAsync`*
*   **Detailed Network Connection Timings:** Measures each phase of the connection setup as an individual audit:
    - **`DNS Lookup Time`**
    - **`TCP Connection Time`**
    - **`Server Response Time (TTFB)`**
    - **`Content Download Time`**
    - **`Total Network Time`**
*   **DOM Size:** Checks if the document exceeds the Lighthouse threshold (up to 1500 elements).
*   **Resource Hints:** Checks for `<link rel="preconnect">` or `dns-prefetch` optimization tags.
*   **Minification Check:** Audits if external stylesheets and scripts are properly minified (`.min.css` / `.min.js`).
*   **HTML Size:** Validates whether the HTML size is within acceptable limits (up to 600 KB).
*   **Image Dimensions & Delivery:** Image Width/Height (CLS checks), Lazy Loading presence, and Modern Image Formats (WebP/AVIF).

### 3. Security — *`Seo.FromUrlAsync`*
*   **HTTPS Usage:** Validates whether the website utilizes SSL/TLS transport protocol.
*   **Insecure Resources (Mixed Content):** Ensures that an HTTPS page does not fetch insecure HTTP assets.
*   **Secure Password Forms:** If a password input field exists, ensures that the hosting page and the submitting form action use secure HTTPS endpoints.
*   **Content Security Policy (CSP):** Audits for the presence of a CSP declaration meta tag.
*   **HTTP Security Headers:** Audits response headers for security protection configurations (`Strict-Transport-Security`, `X-Frame-Options`, `X-Content-Type-Options`, and `Referrer-Policy`).
*   **TLS Protocol Version:** Performs a secure connection test handshake to verify the server uses modern TLS 1.2 or 1.3 protocol.
*   **External Link Target Security:** Audits `target="_blank"` links for missing `rel="noopener"` or `rel="noreferrer"` attributes.

---

## Response Structure

### Data Models

#### `SeoResult`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Score` | `int` | SEO score from 0 to 100. |
| `Audits` | `List<SeoAudit>` | List of SEO audits executed. |

#### `AnalysisResult`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Score` | `int` | Rating from 0 to 100 representing the simple average of SEO, Performance, and Security scores. |
| `SeoScore` | `int` | SEO specific score from 0 to 100. |
| `PerformanceScore` | `int` | Performance specific score from 0 to 100. |
| `SecurityScore` | `int` | Security specific score from 0 to 100. |
| `Audits` | `List<SeoAudit>` | List of all executed checks. |

#### `SeoAudit`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string` | The name of the audit performed. |
| `Passed` | `bool` | `true` if the check met the requirements, otherwise `false`. |
| `Value` | `string?` | The found value or a brief textual summary. |
| `Recommendation`| `string?` | Actionable fix suggestion (omitted in JSON if null). |
| `Weight` | `int` | The importance weight used to calculate the category score. |
| `Details` | `object?` | Structured diagnostic data (omitted in JSON if null). |
| `Category` | `AuditCategory`| The category of this audit: `Seo`, `Performance`, or `Security`. |

---

### Structured Details Types

When an audit fails or requires deeper diagnostics, the `Details` property contains one of the following concrete types depending on the audit:

*   **`HeadingAuditItem`**: Contains the hierarchy and count of heading tags (`H1` to `H6`).
*   **`MetaTagAuditItem`**: Provides a breakdown of specific social tags (`Open Graph` or `Twitter Cards`) detailing which ones were found.
*   **`TagAuditItem`**: Lists obsolete or deprecated HTML tags found within the document.
*   **`AttributeAuditItem`**: Lists deprecated HTML attributes used in the document markup.
*   **`List<string>`**: A flat list of paths, URIs, or header keys that failed the audit (e.g., image URLs missing alt/dimensions, render-blocking scripts, unminified resources, or risky external links).

## Requirements
.NET 9.0 SDK or higher.
