# SeoAnalyzer

A lightweight, high-performance C# library designed for deep on-page **SEO**, **Performance**, and **Security** analysis. Built on top of `AngleSharp`, it fast-parses raw HTML or live URLs to deliver structured, production-ready diagnostic reports complete with automated audits, actionable fix recommendations, and segmented scores.

## Installation

```bash
dotnet add package SeoAnalyzer
```

## Usage

The library provides four entry points:
1. **`Seo.FromHtmlAsync(html, url)`**: Audits **SEO** metrics on raw HTML, returning a `SeoResult` (includes `Tech`).
2. **`Seo.FromUrlAsync(url)`**: Fetches a live URL and audits **SEO**, **Performance**, and **Security**, returning an `AnalysisResult` (includes `Tech`). Cookies set by the server are captured automatically.
3. **`Seo.AnalyzeAsync(pageContext)`**: Runs a full audit from a pre-built `PageContext`, returning an `AnalysisResult` (includes `Tech`). Integrates with Playwright, Selenium, Puppeteer, etc.
4. **`Seo.DetectTechAsync(html, cookies?)`**: Detects the technology stack only - no SEO/Performance/Security audits - returning a `TechResult` directly.

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

    // Tech stack is always available directly
    Console.WriteLine($"CMS: {string.Join(", ", result.Tech?.Platforms ?? [])}");
    Console.WriteLine($"JS: {string.Join(", ", result.Tech?.JsFrameworks ?? [])}");
}
```

### 3. Technology Detection Only

Use `Seo.DetectTechAsync` when you only need the technology stack without running a full audit.

```csharp
using SeoAnalyzer;

var tech = await Seo.DetectTechAsync(html);

Console.WriteLine($"Platforms : {string.Join(", ", tech.Platforms)}");
Console.WriteLine($"JS        : {string.Join(", ", tech.JsFrameworks)}");
Console.WriteLine($"CSS       : {string.Join(", ", tech.CssFrameworks)}");
Console.WriteLine($"reCAPTCHA : {tech.HasRecaptcha}");
```


### 4. Custom Page Context (Playwright, Selenium, etc.)

Use `Seo.AnalyzeAsync(PageContext)` when you already have the page loaded in a browser or from any external source.
Pass `Cookies` to enable cookie-based platform detection - typically extracted from the browser's cookie store.

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

// Extract cookies from the browser context for platform detection
var browserCookies = await page.Context.CookiesAsync();
var cookieString = string.Join("; ", browserCookies.Select(c => $"{c.Name}={c.Value}"));

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
    ResponseHeaders = httpResponse.Headers,
    Cookies = cookieString  // cookies from the browser's cookie store
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

### 2. Technology Detection - *all entry points, also standalone via `DetectTechAsync`*

Available as a dedicated `TechResult` object on every result and via `Seo.DetectTechAsync`.
*   **CMS / Site Builder:** WordPress, Wix, Shopify, Squarespace, Webflow, Joomla, Drupal, Umbraco, TYPO3, Blogger, Weebly, Jimdo, GoDaddy Builder, Hostinger, Duda, NuvemShop, LojaIntegrada, Tray, Hotmart, Kiwify, Cartpanda.
*   **JavaScript Frameworks:** React, Next.js, Vue.js, Nuxt.js, Angular, Svelte, Ember.js, Backbone.js, Alpine.js, HTMX, jQuery, Stimulus, Lit.
*   **CSS Frameworks / Libraries:** Bootstrap, Tailwind CSS, Bulma, Foundation, Materialize CSS, Semantic UI.
*   **reCAPTCHA:** Detects Google reCAPTCHA v2, v3, and Enterprise via script URLs and DOM signals.

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
| `Score` | `int` | SEO score from 0 to 100 calculated as unweighted average (Passed = 1.0, Warning = 0.5, Failed = 0.0). `Info` audits are excluded from the score. |
| `TotalPassed` | `int` | Count of SEO audits that passed. |
| `TotalFailed` | `int` | Count of SEO audits that failed. |
| `TotalWarnings` | `int` | Count of SEO audits with Warning status. |
| `Audits` | `List<SeoAudit>` | List of SEO audits executed (includes `Info` audits). |
| `Tech` | `TechResult?` | Detected technology stack. |

#### `AnalysisResult`
| Property | Type | Description |
| :--- | :--- | :--- |
| `TotalScore` | `int` | Rating from 0 to 100 representing the simple average of SEO, Performance, and Security scores. |
| `Seo` | `CategorySummary?` | Detailed metrics for SEO category. |
| `Performance` | `CategorySummary?` | Detailed metrics for Performance category. |
| `Security` | `CategorySummary?` | Detailed metrics for Security category. |
| `Tech` | `TechResult?` | Detected technology stack. |

#### `CategorySummary`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Score` | `int` | Category score from 0 to 100 calculated as unweighted average. `Info` audits do not count toward this score. |
| `TotalPassed` | `int` | Count of audits that passed. |
| `TotalFailed` | `int` | Count of audits that failed. |
| `TotalWarnings` | `int` | Count of audits with Warning status. |
| `Audits` | `List<SeoAudit>` | List of all audits executed in this category, including `Info` audits. |

#### `SeoAudit`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string` | The name of the audit performed. |
| `Status` | `AuditStatus` | Status of the audit: `Passed`, `Failed`, `Warning`, or `Info`. |
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
| `Cookies` | `string?` | Optional. Raw cookie string (e.g. `"name=value; name2=value2"`). Used as a last-resort signal for CMS / platform detection. When using `FromUrlAsync`, this is populated automatically from `Set-Cookie` response headers. When using `AnalyzeAsync` with a browser tool, pass the cookies extracted from the browser's cookie store. |

---

### `AuditStatus` Values

| Value | Meaning | Affects Score? |
| :--- | :--- | :---: |
| `Passed` | Audit check succeeded. | ✅ Yes (1.0 weight) |
| `Warning` | Check has concerns but is not a hard failure. | ✅ Yes (0.5 weight) |
| `Failed` | Audit check failed. | ✅ Yes (0.0 weight) |
| `Info` | Informational result only (e.g., technology detection). | ❌ No |

---

### Structured Details Types

When an audit fails or requires deeper diagnostics, the `Details` property contains one of the following concrete types depending on the audit:

*   **`HeadingAuditItem`**: Contains the hierarchy and count of heading tags (`H1` to `H6`).
*   **`TagAuditItem`**: Lists obsolete or deprecated HTML tags found within the document.
*   **`AttributeAuditItem`**: Lists deprecated HTML attributes used in the document markup.
*   **`List<string>`**: A flat list of paths, URIs, header keys, or detected technology names (e.g., image URLs missing alt/dimensions, scripts, unminified resources, risky external links, or detected frameworks/platforms).

#### `TechResult`
| Property | Type | Description |
| :--- | :--- | :--- |
| `Platforms` | `List<string>` | Detected CMS or site builder names. Platforms detected only via cookies are suffixed with `(via cookies)`. Empty if none detected. |
| `JsFrameworks` | `List<string>` | Detected JavaScript framework names. Empty if none detected. |
| `CssFrameworks` | `List<string>` | Detected CSS framework / library names. Empty if none detected. |
| `HasRecaptcha` | `bool` | `true` if any Google reCAPTCHA signal was found. |

---

## Requirements
.NET 9.0 SDK or higher.