using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace SeoAnalyzer.Helpers;

internal class ImageHelper
{
    
    private static readonly FrozenSet<string> _modernExtensions =
        FrozenSet.ToFrozenSet<string>([".webp", ".avif", ".svg"]);

    public static bool IsModernFormat(IHtmlImageElement img)
    {
        var src = img.GetAttribute("src");
        if (string.IsNullOrWhiteSpace(src)) return false;

        var path = src.Contains('?') ? src[..src.IndexOf('?')] : src;
        return _modernExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
