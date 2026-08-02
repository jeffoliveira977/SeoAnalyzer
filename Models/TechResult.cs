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
    /// <summary>
    /// Detected CMS or site builder names.
    /// Platform detected exclusively via cookies will be suffixed with "(via cookies)".
    /// Empty if none was detected.
    /// </summary>
    public List<string> Platforms { get; set; } = [];

    /// <summary>
    /// Detected JavaScript framework names (e.g. "React", "Vue.js", "Next.js").
    /// Empty if none was detected.
    /// </summary>
    public List<string> JsFrameworks { get; set; } = [];

    /// <summary>
    /// Detected CSS framework / library names (e.g. "Bootstrap", "Tailwind CSS").
    /// Empty if none was detected.
    /// </summary>
    public List<string> CssFrameworks { get; set; } = [];

    /// <summary>
    /// <see langword="true"/> if any Google reCAPTCHA signal was found on the page.
    /// </summary>
    public bool HasRecaptcha { get; set; }
}
