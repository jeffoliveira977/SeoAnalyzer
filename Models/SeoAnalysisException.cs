namespace SeoAnalyzer.Models;

public class SeoAnalysisException : Exception
{
    public SeoAnalysisException(string message) : base(message) { }
    public SeoAnalysisException(string message, Exception inner) : base(message, inner) { }
}