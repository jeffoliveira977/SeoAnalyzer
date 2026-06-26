namespace SeoAnalyzer;

[Flags]
public enum SeoRules
{
    None = 0,
    Metadata = 1 << 0,
    Headings = 1 << 1,
    Links = 1 << 2,
    HtmlStructure = 1 << 3,
    StructuredData = 1 << 4,
    Indexing = 1 << 5,
    Social = 1 << 6,
    TagManager = 1 << 7,
    DeprecatedHtml = 1 << 8,
    CommonKeywords = 1 << 9,
    Images = 1 << 10,

    All = ~0
}