using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using SeoAnalyzer.Helpers;
using SeoAnalyzer.Models;
using SeoAnalyzer.Rules.Performance;
using SeoAnalyzer.Rules.Security;
using SeoAnalyzer.Rules.SEO;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeoAnalyzer;

public static class Seo
{
    private static readonly HtmlParser _parser = new();

    /// <summary>
    /// Analyzes a static raw HTML string. Limited to structural on-page SEO audits.
    /// </summary>
    public static async Task<SeoResult?> FromHtmlAsync(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return null;

        var document = await _parser.ParseDocumentAsync(html);

        var images = document.Images.OfType<IElement>().Where(e => UrlHelper.IsImageUrl(e.GetAttribute("src"))).ToList();
        var links = document.Links.OfType<IElement>().ToList();
        var scripts = document.Scripts.OfType<IElement>().ToList();

        var audits = await RunSeoAuditsAsync(document, images, links, scripts);

        var seo = ScoreCalculator.Calculate(audits, AuditCategory.Seo);

        return new SeoResult
        {
            Audits = audits,
            Score = seo
        };
    }

    /// <summary>
    /// Fetches a live URL to perform a complete audit, including network performance, 
    /// server security headers, and full on-page SEO.
    /// </summary>
    public static async Task<AnalysisResult?> FromUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var fetchResult = await NetworkTimerService.FetchAndMeasureAsync(url);
        if (fetchResult == null) return null;

        var document = await _parser.ParseDocumentAsync(fetchResult.Html);

        var images = document.Images.OfType<IElement>().Where(e => UrlHelper.IsImageUrl(e.GetAttribute("src"))).ToList();
        var links = document.Links.OfType<IElement>().ToList();
        var scripts = document.Scripts.OfType<IElement>().ToList();

        var audits = await RunSeoAuditsAsync(document, images, links, scripts);

        audits.AddRange(await RunPerformanceAuditsAsync(document, fetchResult, url, images, links, scripts));
        audits.AddRange(await RunSecurityAuditsAsync(document, fetchResult, url, images, links, scripts));

        var seo = ScoreCalculator.Calculate(audits, AuditCategory.Seo);
        var performance = ScoreCalculator.Calculate(audits, AuditCategory.Performance);
        var security = ScoreCalculator.Calculate(audits, AuditCategory.Security);

        return new AnalysisResult
        {
            Audits = audits,
            SeoScore = seo,
            PerformanceScore = performance,
            SecurityScore = security,
            Score = (seo + performance + security) / 3
        };
    }

    private static async Task<List<SeoAudit>> RunSeoAuditsAsync(
        IDocument document, List<IElement> images, List<IElement> links, List<IElement> scripts)
    {
        TextHelper.LoadStopwords(HtmlLangDetector.Detect(document));

        var audits = new List<SeoAudit>();
        audits.AddRange(MetadataRules.Execute(document, links));
        audits.AddRange(HeadingRules.Execute(document));
        audits.AddRange(LinkRules.Execute(document));
        audits.AddRange(HtmlRules.Execute(links));
        audits.AddRange(StructuredDataRules.Execute(document));
        audits.AddRange(SocialRules.Execute(document));
        audits.AddRange(TagManagerRules.Execute(scripts, document));
        audits.AddRange(CommonKeywordsRules.Execute(document));
        audits.AddRange(ImageRules.Execute(images));
        audits.AddRange(await IndexingRules.ExecuteAsync(document));
        return audits;
    }

    private static async Task<List<SeoAudit>> RunPerformanceAuditsAsync(
        IDocument document, NetworkFetchResult fetchResult, string url, List<IElement> images, List<IElement> links, List<IElement> scripts)
    {
        var audits = new List<SeoAudit>
        {
            DomSizeRules.Execute(document),
            ResourceHintsRules.Execute(links)
        };

        audits.AddRange(await MinificationRules.ExecuteAsync(scripts, links, url));
        audits.AddRange(DeprecatedHtmlRules.Execute(document));
        audits.AddRange(HtmlSizeRules.Execute(document));
        audits.AddRange(ImagePerformanceRules.Execute(images));
        audits.AddRange(NetworkTimeRules.Execute(fetchResult.Metrics));
        return audits;
    }

    private static async Task<List<SeoAudit>> RunSecurityAuditsAsync(
        IDocument document, NetworkFetchResult fetchResult, string url, List<IElement> images, List<IElement> links, List<IElement> scripts)
    {

        var metas = document.QuerySelectorAll("meta").OfType<IElement>().ToList();

        var audits = new List<SeoAudit>();

        var https = HttpsUsageRules.Execute(url);
        var mixedContent = InsecureResources.Execute(scripts, links, images, metas, url);
        var securePass = SecurePasswordRules.Execute(document, url);
        var headers = SecurityHeadersRules.Execute(fetchResult.ResponseHeaders);
        var tls = await TlsVersionRules.ExecuteAsync(url);

        if (https != null) audits.Add(https);
        if (mixedContent != null) audits.Add(mixedContent);
        if (securePass != null) audits.Add(securePass);
        if (headers != null) audits.Add(headers);
        if (tls != null) audits.Add(tls);

        audits.Add(CspRules.Execute(document));
        audits.Add(ExternalLinksSecurityRules.Execute(document));
        return audits;
    }

}