using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Performance;

/// <summary>Audits detailed network performance metrics (DNS, TCP, TTFB, Download, Total).</summary>
internal static class NetworkTimeRules
{
    public static List<SeoAudit> Execute(NetworkPerformanceMetrics metrics)
    {
        var audits = new List<SeoAudit>();

        // DNS Lookup
        var dnsPassed = metrics.DnsLookupMs < 150;
        audits.Add(new SeoAudit
        {
            Title = "DNS Lookup Time",
            Passed = dnsPassed,
            Value = $"{metrics.DnsLookupMs:F2} ms",
            Weight = 1,
            Recommendation = dnsPassed ? null : "Optimize DNS resolution times by using a fast DNS provider or CDN.",
            Category = AuditCategory.Performance
        });

        // TCP Connection
        var tcpPassed = metrics.TcpConnectionMs < 150;
        audits.Add(new SeoAudit
        {
            Title = "TCP Connection Time",
            Passed = tcpPassed,
            Value = $"{metrics.TcpConnectionMs:F2} ms",
            Weight = 1,
            Recommendation = tcpPassed ? null : "TCP connection time is high. Consider using CDNs or edge locations to reduce network latency.",
            Category = AuditCategory.Performance
        });

        // Server Response Time (TTFB)
        var ttfbPassed = metrics.TtfbMs < 800;
        audits.Add(new SeoAudit
        {
            Title = "Server Response Time (TTFB)",
            Passed = ttfbPassed,
            Value = $"{metrics.TtfbMs:F2} ms",
            Weight = 5,
            Recommendation = ttfbPassed ? null : "Initial server response time (TTFB) is high. Optimize database queries, use CDN caching, or upgrade server hosting resources.",
            Category = AuditCategory.Performance
        });

        // Content Download
        var downloadPassed = metrics.ContentDownloadMs < 500;
        audits.Add(new SeoAudit
        {
            Title = "Content Download Time",
            Passed = downloadPassed,
            Value = $"{metrics.ContentDownloadMs:F2} ms",
            Weight = 1,
            Recommendation = downloadPassed ? null : "HTML content download time is high. Minify HTML, compress text payloads (GZip/Brotli), or optimize server bandwidth.",
            Category = AuditCategory.Performance
        });

        // Total Network Time
        var totalPassed = metrics.TotalNetworkTimeMs < 3000;
        audits.Add(new SeoAudit
        {
            Title = "Total Network Time",
            Passed = totalPassed,
            Value = $"{metrics.TotalNetworkTimeMs:F2} ms",
            Weight = 2,
            Recommendation = totalPassed ? null : "Total network execution time is high. Address latency, connection setup, and server processing times.",
            Category = AuditCategory.Performance
        });

        return audits;
    }
}
