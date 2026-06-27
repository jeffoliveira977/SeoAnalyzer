namespace SeoAnalyzer.Models;

/// <summary>Represents detailed network performance metrics for page loading.</summary>
public class NetworkPerformanceMetrics
{
    public double DnsLookupMs { get; set; }
    public double TcpConnectionMs { get; set; }
    public double TtfbMs { get; set; }
    public double ContentDownloadMs { get; set; }
    public double TotalNetworkTimeMs { get; set; }
}
