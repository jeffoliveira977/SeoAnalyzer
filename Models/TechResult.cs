using System.Collections.Generic;

namespace SeoAnalyzer.Models;

/// <summary>
/// Technology stack detected on the page.
/// Populated automatically by <see cref="Seo.AnalyzeAsync"/>, <see cref="Seo.FromUrlAsync"/>,
/// and <see cref="Seo.FromHtmlAsync"/>, and also available as a standalone result via
/// <see cref="Seo.DetectTechAsync"/>.
/// </summary>
public sealed class TechResult
{
    public List<string> Platforms { get; set; } = [];
    public List<string> JsFrameworks { get; set; } = [];
    public List<string> CssFrameworks { get; set; } = [];
    public bool HasRecaptcha { get; set; }
}
