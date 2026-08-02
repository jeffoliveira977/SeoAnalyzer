using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;
using SeoAnalyzer.Rules.Performance;
using SeoAnalyzer.Rules.Security;
using SeoAnalyzer.Rules.SEO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeoAnalyzer;

public static class Seo
{
    private static readonly HtmlParser _parser = new();

    private record PageElements(
        IDocument Document,
        List<IHtmlImageElement> Images,
        List<IHtmlAnchorElement> Links,
        List<IHtmlScriptElement> Scripts,
        List<IHtmlLinkElement> HeadLinks
    );

    /// <summary>
    /// Analyzes a static raw HTML string. Limited to structural on-page SEO audits.
    /// </summary>
    public static async Task<SeoResult?> FromHtmlAsync(string html, string url, bool enableLog = true)
    {
        if (string.IsNullOrWhiteSpace(html))
            throw new ArgumentException("HTML cannot be empty.", nameof(html));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));

        LogHelper.Info($"Analyzing HTML for '{url}'...", enableLog);

        var page = await ParseDocumentAsync(html);
        var pageUrl = UrlHelper.NormalizeUrl(url);

        var audits = await RunSeoAuditsAsync(page, pageUrl);
        var summary = ScoreCalculator.BuildSummary(audits, AuditCategory.Seo)
                   ?? throw new SeoAnalysisException("No SEO audits were produced. The HTML may be empty or malformed.");

        LogHelper.Success($"HTML analysis completed. SEO Score: {summary.Score}/100.", enableLog);

        return new SeoResult
        {
            TotalPassed = summary.TotalPassed,
            TotalFailed = summary.TotalFailed,
            TotalWarnings = summary.TotalWarnings,
            Score = summary.Score,
            Audits = audits,
            Tech = TechRules.Detect(page.Document, page.Scripts)
        };
    }

    /// <summary>
    /// Detects the technology stack (CMS, JS/CSS frameworks, reCAPTCHA) from a raw HTML string
    /// without running any SEO, Performance, or Security audits.
    /// </summary>
    /// <param name="html">The raw HTML content of the page.</param>
    /// <param name="cookies">
    /// Optional raw cookie string (format: "name=value; name2=value2").
    /// Used as a last-resort signal for CMS / platform detection.
    /// </param>
    /// <param name="enableLog">Whether to log progress messages to standard output.</param>
    public static async Task<TechResult> DetectTechAsync(string html, string? cookies = null, bool enableLog = true)
    {
        if (string.IsNullOrWhiteSpace(html))
            throw new ArgumentException("HTML cannot be empty.", nameof(html));

        LogHelper.Info("Detecting tech stack...", enableLog);

        var page = await ParseDocumentAsync(html);
        var tech = TechRules.Detect(page.Document, page.Scripts, cookies);

        var detected = tech.Platforms.Concat(tech.JsFrameworks).Concat(tech.CssFrameworks).ToList();
        var summaryText = detected.Count > 0 ? string.Join(", ", detected) : "None detected";

        LogHelper.Success($"Tech stack detection completed ({summaryText}).", enableLog);

        return tech;
    }

    /// <summary>
    /// Fetches a live URL to perform a complete audit, including network performance, 
    /// server security headers, and full on-page SEO.
    /// </summary>
    public static async Task<AnalysisResult?> FromUrlAsync(string url, bool enableLog = true)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));

        LogHelper.Info($"Analyzing URL: '{url}'...", enableLog);

        var normalized = UrlHelper.NormalizeUrl(url);

        var result = await NetworkTimerService.FetchAndMeasureAsync(normalized);
        if (result == null)
        {
            LogHelper.Error($"Failed to fetch '{normalized}' - site unreachable or timing out.", enableLog);
            return null;
        }

        return await AnalyzeAsync(new PageContext
        {
            Html = result.Html,
            Url = normalized,
            ResponseHeaders = result.ResponseHeaders,
            Metrics = result.Metrics,
            Cookies = result.Cookies
        }, enableLog);
    }

    /// <summary>
    /// Runs a full audit from a pre-built <see cref="PageContext"/>.
    /// Use this when you already have the HTML and metrics from an external source
    /// (e.g. Playwright, Selenium, Puppeteer, or any custom HTTP client).
    /// </summary>
    public static async Task<AnalysisResult?> AnalyzeAsync(PageContext context, bool enableLog = false)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(context.Html))
            throw new ArgumentException("PageContext.Html cannot be empty.", nameof(context));

        if (string.IsNullOrWhiteSpace(context.Url))
            throw new ArgumentException("PageContext.Url cannot be empty.", nameof(context));

        LogHelper.Info($"Running audit checks for '{context.Url}'...", enableLog);

        var page = await ParseDocumentAsync(context.Html);
        var pageUrl = UrlHelper.NormalizeUrl(context.Url);

        var updated = context with { Url = pageUrl };

        var sw = System.Diagnostics.Stopwatch.StartNew();

        var seoAudits = await RunSeoAuditsAsync(page, pageUrl, context.Cookies);
        LogHelper.Info($"[Timing] SEO audits for '{pageUrl}': {sw.ElapsedMilliseconds}ms", enableLog);
        sw.Restart();

        var perfAudits = await RunPerformanceAuditsAsync(page, updated);
        LogHelper.Info($"[Timing] Performance audits for '{pageUrl}': {sw.ElapsedMilliseconds}ms", enableLog);
        sw.Restart();

        var secAudits = await RunSecurityAuditsAsync(page, updated);
        LogHelper.Info($"[Timing] Security audits for '{pageUrl}': {sw.ElapsedMilliseconds}ms", enableLog);


        var audits = new List<SeoAudit>();
        audits.AddRange(seoAudits);
        audits.AddRange(perfAudits);
        audits.AddRange(secAudits);

        if (audits.Count == 0)
            throw new SeoAnalysisException("No audits were produced. The HTML may be malformed or empty.");

        var seo = ScoreCalculator.BuildSummary(audits, AuditCategory.Seo);
        var performance = ScoreCalculator.BuildSummary(audits, AuditCategory.Performance);
        var security = ScoreCalculator.BuildSummary(audits, AuditCategory.Security);

        var scores = new[] { seo?.Score, performance?.Score, security?.Score }
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();

        if (scores.Count == 0)
            throw new SeoAnalysisException("Score could not be calculated — no valid audit categories found.");

        var totalScore = (int)Math.Round(scores.Average());
        var tech = TechRules.Detect(page.Document, page.Scripts, context.Cookies);

        LogHelper.Success($"Analysis completed. Total Score: {totalScore}/100 (SEO: {seo?.Score ?? 0}, Perf: {performance?.Score ?? 0}, Sec: {security?.Score ?? 0}).", enableLog);

        return new AnalysisResult
        {
            Seo = seo,
            Performance = performance,
            Security = security,
            TotalScore = totalScore,
            Tech = tech
        };
    }

    private static async Task<PageElements> ParseDocumentAsync(string html)
    {
        var document = await _parser.ParseDocumentAsync(html);
        return new PageElements(
            document,
            [.. document.Images.OfType<IHtmlImageElement>().Where(e => UrlHelper.IsImageUrl(e.GetAttribute("src")))],
            [.. document.Links.OfType<IHtmlAnchorElement>()],
            [.. document.Scripts.OfType<IHtmlScriptElement>()],
            [.. document.QuerySelectorAll("link").OfType<IHtmlLinkElement>()]
        );
    }

    private static async Task<List<SeoAudit>> RunSeoAuditsAsync(PageElements page, string url, string? cookies = null)
    {
        var stopwords = TextHelper.BuildStopwords(HtmlLangDetector.Detect(page.Document));

        var audits = new List<SeoAudit>();
        audits.AddRange(MetadataRules.Execute(page.Document, page.HeadLinks));
        audits.AddRange(HeadingRules.Execute(page.Document));
        audits.AddRange(LinkRules.Execute(page.Links, url));
        audits.AddRange(HtmlRules.Execute(page.Links));
        audits.AddRange(StructuredDataRules.Execute(page.Document));
        audits.AddRange(SocialRules.Execute(page.Document));
        audits.AddRange(TagManagerRules.Execute(page.Document, page.Scripts));
        audits.AddRange(CommonKeywordsRules.Execute(page.Document, stopwords));
        audits.AddRange(ImageAltRules.Execute(page.Images));
        audits.AddRange(await IndexingRules.ExecuteAsync(page.Document, url));
        audits.AddRange(TechRules.Execute(page.Document, page.Scripts, cookies));
        return audits;
    }

    private static async Task<List<SeoAudit>> RunPerformanceAuditsAsync(PageElements page, PageContext context)
    {
        var audits = new List<SeoAudit>
        {
            DomSizeRules.Execute(page.Document),
            ResourceHintsRules.Execute(page.Scripts, page.HeadLinks, context.Url)
        };

        audits.AddRange(await MinificationRules.ExecuteAsync(page.Scripts, page.HeadLinks, context.Url));
        audits.AddRange(DeprecatedHtmlRules.Execute(page.Document));
        audits.AddRange(HtmlSizeRules.Execute(page.Document));
        audits.AddRange(ImagePerformanceRules.Execute(page.Images, context.Url));

        if (context.Metrics != null)
            audits.AddRange(NetworkTimeRules.Execute(context.Metrics));

        return audits;
    }

    private static async Task<List<SeoAudit>> RunSecurityAuditsAsync(PageElements page, PageContext context)
    {
        var metas = page.Document.QuerySelectorAll("meta").OfType<IElement>().ToList();

        var audits = new List<SeoAudit>();

        var https = HttpsUsageRules.Execute(context.Url);
        var mixedContent = InsecureResources.Execute(page.Scripts, page.Links, page.Images, metas, context.Url);
        var securePass = SecurePasswordRules.Execute(page.Document, context.Url);
        var headers = SecurityHeadersRules.Execute(context.ResponseHeaders);
        var tls = await TlsVersionRules.ExecuteAsync(context.Url);

        if (https != null) audits.Add(https);
        if (mixedContent != null) audits.Add(mixedContent);
        if (securePass != null) audits.Add(securePass);
        if (headers != null) audits.Add(headers);
        if (tls != null) audits.Add(tls);

        audits.Add(CspRules.Execute(page.Document));
        audits.Add(ExternalLinksSecurityRules.Execute(page.Links));
        return audits;
    }
}
