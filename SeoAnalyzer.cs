using AngleSharp.Html.Parser;

namespace SeoAnalyzer;

public static class SeoAnalyzer
{
    private static readonly HtmlParser _parser = new();

    /// <summary>Analyzes raw HTML and returns the full report.</summary>
    public static async Task<SeoAnalysis?> FromHtmlAsync(string html, AnalyzerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var document = await _parser.ParseDocumentAsync(html);

        options ??= new AnalyzerOptions();
        var rules = options.Rules;

        // Stopwords depend on page language
        if (rules.HasFlag(SeoRules.CommonKeywords))
        {
            var lang = HtmlLangDetector.Detect(document);
            TextHelper.LoadStopwords(lang);
        }

        var result = new SeoAnalysis();

        if (rules.HasFlag(SeoRules.Metadata))
            result.Audits.AddRange(MetadataRules.Execute(document));

        if (rules.HasFlag(SeoRules.Headings))
            result.Audits.AddRange(HeadingRules.Execute(document));

        if (rules.HasFlag(SeoRules.Links))
            result.Audits.AddRange(LinkRules.Execute(document));

        if (rules.HasFlag(SeoRules.HtmlStructure))
            result.Audits.AddRange(HtmlRules.Execute(document));

        if (rules.HasFlag(SeoRules.StructuredData))
            result.Audits.AddRange(StructuredDataRules.Execute(document));

        if (rules.HasFlag(SeoRules.Social))
            result.Audits.AddRange(SocialRules.Execute(document));

        if (rules.HasFlag(SeoRules.TagManager))
            result.Audits.AddRange(TagManagerRules.Execute(document));

        if (rules.HasFlag(SeoRules.DeprecatedHtml))
            result.Audits.AddRange(DeprecatedHtmlRules.Execute(document));

        if (rules.HasFlag(SeoRules.CommonKeywords))
            result.Audits.AddRange(CommonKeywordsRules.Execute(document));

        if (rules.HasFlag(SeoRules.Images))
            result.Audits.AddRange(ImageRules.Execute(document));

        if (rules.HasFlag(SeoRules.Indexing))
        {
            var indexingAudits = await IndexingRules.ExecuteAsync(document);
            result.Audits.AddRange(indexingAudits);
        }

        result.Score = ScoreCalculator.Calculate(result);

        return result;
    }

    /// <summary>Fetches the URL and delegates to FromHtmlAsync.</summary>
    public static async Task<SeoAnalysis?> FromUrlAsync(string url, AnalyzerOptions? options = null)
    {
        var response = await UrlHelper.FetchAsync(new Uri(url));
        if (response == null || !response.IsSuccessStatusCode) return null;

        var html = await response.Content.ReadAsStringAsync();
        return await FromHtmlAsync(html, options);
    }
}
