# SeoAnalyzer

A lightweight, high-performance C# library designed for deep on-page **SEO**, **Performance**, and **Security** analysis. Built on top of `AngleSharp`, it fast-parses raw HTML or live URLs to deliver structured, production-ready diagnostic reports complete with automated audits, actionable fix recommendations, and segmented scores.

## Installation

```bash
dotnet add package SeoAnalyzer
```

## Usage

The library provides three entry points:
1. **`Seo.FromHtmlAsync(html, url)`**: Audits **SEO** specific metrics on raw HTML and URL, returning a `SeoResult`.
2. **`Seo.FromUrlAsync(url)`**: Fetches a live URL and audits **SEO**, **Performance**, and **Security**, returning an `AnalysisResult`.
3. **`Seo.AnalyzeAsync(pageContext)`**: Runs a full audit from a pre-built `PageContext`, allowing integration with any browser automation tool (Playwright, Selenium, Puppeteer, etc.).

### 1. SEO Analysis from HTML String

```csharp
using SeoAnalyzer;


var result = await Seo.FromHtmlAsync(html, url);

if (result != null)
{
    Console.WriteLine($"SEO Score: {result.Score}/100");
    foreach (var audit in result.Audits)
    {
        Console.WriteLine($"[{audit.Status}] {audit.Title}: {audit.Value}");
    }
}
```

### 2. Full URL Analysis (SEO, Performance & Security)

```csharp
using SeoAnalyzer;

var result = await Seo.FromUrlAsync("https://www.amazon.com.br");

if (result != null)
{
    Console.WriteLine($"Overall Score: {result.TotalScore}/100");
    Console.WriteLine($"SEO: {result.Seo?.Score}/100 ({result.Seo?.TotalPassed} passed, {result.Seo?.TotalFailed} failed, {result.Seo?.TotalWarnings} warnings)");
    Console.WriteLine($"Performance: {result.Performance?.Score}/100");
    Console.WriteLine($"Security: {result.Security?.Score}/100");
}
```

### 3. Custom Page Context (Playwright, Selenium, etc.)

Use `Seo.AnalyzeAsync(PageContext)` when you already have the page loaded in a browser or from any external source.

#### Playwright Example

```csharp
using SeoAnalyzer;
using SeoAnalyzer.Models;
using Microsoft.Playwright;
using System.Text.Json;

var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();
var response = await page.GotoAsync("https://www.amazon.com.br");

var html = await page.ContentAsync();

// Extract real network metrics from the browser's Navigation Timing API
var timing = await page.EvaluateAsync<JsonElement>(@"() => {
    const e = performance.getEntriesByType('navigation')[0];
    return {
        dns: e.domainLookupEnd - e.domainLookupStart,
        tcp: e.connectEnd - e.connectStart,
        ttfb: e.responseStart - e.requestStart,
        download: e.responseEnd - e.responseStart,
        total: e.responseEnd - e.startTime
    };
}");

// Convert Playwright response headers to HttpResponseHeaders
var httpResponse = new HttpResponseMessage();
foreach (var header in response!.Headers)
    httpResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);

var result = await Seo.AnalyzeAsync(new PageContext
{
    Html = html,
    Url = page.Url,
    Metrics = new NetworkPerformanceMetrics
    {
        DnsLookupMs = timing.GetProperty("dns").GetDouble(),
        TcpConnectionMs = timing.GetProperty("tcp").GetDouble(),
        TtfbMs = timing.GetProperty("ttfb").GetDouble(),
        ContentDownloadMs = timing.GetProperty("download").GetDouble(),
        TotalNetworkTimeMs = timing.GetProperty("total").GetDouble()
    },
    ResponseHeaders = httpResponse.Headers
});

Console.WriteLine($"Score: {result?.TotalScore}/100");

await browser.CloseAsync();
```

---

## What is Analyzed?

### 1. Search Engine Optimization (SEO) - *`FromHtmlAsync`, `FromUrlAsync`, `AnalyzeAsync`*
*   **Metadata:** Title, Description, Canonical Tag, Robots, Viewport, Charset, Lang Attribute, and Favicon.
*   **Content & Structure:** Single H1 Tag, Heading Hierarchy (fails if no H2 or H3 tags are present), and Common Keywords Presence.
*   **Links:** Internal vs External Links ratio and Empty Anchor Text.
*   **Images:** Missing Alt Text (details return a list of image URLs).
*   **Social & Structured Data:** Open Graph (Facebook/LinkedIn), Twitter Cards, and JSON-LD/Microdata Structured Data.
*   **Technical SEO:** Robots.txt, XML Sitemap, and Google Tag Manager (Scripts, Noscripts, dataLayer).

### 2. Performance - *`FromUrlAsync`, `AnalyzeAsync`*
*   **Detailed Network Connection Timings** *(requires `Metrics` in `PageContext`)*:
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

### 3. Security - *`FromUrlAsync`, `AnalyzeAsync`*
*   **HTTPS Usage:** Validates whether the website utilizes SSL/TLS transport protocol.
*   **Insecure Resources (Mixed Content):** Ensures that an HTTPS page does not fetch insecure HTTP assets.
*   **Secure Password Forms:** If a password input field exists, ensures that the hosting page and the submitting form action use secure HTTPS endpoints.
*   **Content Security Policy (CSP):** Audits for the presence of a CSP declaration meta tag.
*   **HTTP Security Headers** *(requires `ResponseHeaders` in `PageContext`)*: Audits response headers for security protection configurations (`Strict-Transport-Security`, `X-Frame-Options`, `X-Content-Type-Options`, and `Referrer-Policy`).
*   **TLS Protocol Version:** Performs a secure connection test handshake to verify the server uses modern TLS 1.2 or 1.3 protocol.
*   **External Link Target Security:** Audits `target="_blank"` links for missing `rel="noopener"` or `rel="noreferrer"` attributes.

---

## Response Structure

### Data Models

#### `SeoResult`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Score` | `int` | SEO score from 0 to 100 calculated as unweighted average (Passed = 1.0, Warning = 0.5, Failed = 0.0). |
| `TotalPassed` | `int` | Count of SEO audits that passed. |
| `TotalFailed` | `int` | Count of SEO audits that failed. |
| `TotalWarnings` | `int` | Count of SEO audits with Warning status. |
| `Audits` | `List<SeoAudit>` | List of SEO audits executed. |

#### `AnalysisResult`
| Property | Type | Description |
| :--- | :--- | :--- |
| `TotalScore` | `int` | Rating from 0 to 100 representing the simple average of SEO, Performance, and Security scores. |
| `Seo` | `CategorySummary?` | Detailed metrics for SEO category. |
| `Performance` | `CategorySummary?` | Detailed metrics for Performance category. |
| `Security` | `CategorySummary?` | Detailed metrics for Security category. |

#### `CategorySummary`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Score` | `int` | Category score from 0 to 100 calculated as unweighted average. |
| `TotalPassed` | `int` | Count of audits that passed. |
| `TotalFailed` | `int` | Count of audits that failed. |
| `TotalWarnings` | `int` | Count of audits with Warning status. |
| `Audits` | `List<SeoAudit>` | List of audits executed in this category. |

#### `SeoAudit`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string` | The name of the audit performed. |
| `Status` | `AuditStatus` | Status of the audit: `Passed`, `Failed`, or `Warning`. |
| `Value` | `string?` | The found value or a brief textual summary. |
| `Recommendation`| `string?` | Actionable fix suggestion (omitted in JSON if null). |
| `Details` | `object?` | Structured diagnostic data (omitted in JSON if null or empty). |
| `Category` | `AuditCategory`| The category of this audit: `Seo`, `Performance`, or `Security`. |

#### `PageContext`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Html` | `string` | **Required.** The raw HTML content of the page. |
| `Url` | `string` | **Required.** The final URL of the page (after redirects). |
| `ResponseHeaders` | `HttpResponseHeaders?` | Optional. HTTP response headers for security header audits. |
| `Metrics` | `NetworkPerformanceMetrics?` | Optional. Network timing metrics for performance audits. |

---

### Structured Details Types

When an audit fails or requires deeper diagnostics, the `Details` property contains one of the following concrete types depending on the audit:

*   **`HeadingAuditItem`**: Contains the hierarchy and count of heading tags (`H1` to `H6`).
*   **`TagAuditItem`**: Lists obsolete or deprecated HTML tags found within the document.
*   **`AttributeAuditItem`**: Lists deprecated HTML attributes used in the document markup.
*   **`List<string>`**: A flat list of paths, URIs, or header keys that failed the audit (e.g., image URLs missing alt/dimensions, scripts, unminified resources, or risky external links).

## Requirements
.NET 9.0 SDK or higher.
