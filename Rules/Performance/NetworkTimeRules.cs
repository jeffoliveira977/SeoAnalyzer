using SeoAnalyzer.Models;
using System.Net.NetworkInformation;

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
            Status = dnsPassed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = $"{metrics.DnsLookupMs:F2} ms",
            Recommendation = dnsPassed ? null : "Optimize DNS resolution times by using a fast DNS provider or CDN.",
            Category = AuditCategory.Performance
        });

        // TCP Connection
        var tcpPassed = metrics.TcpConnectionMs < 150;
        audits.Add(new SeoAudit
        {
            Title = "TCP Connection Time",
            Status = tcpPassed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = $"{metrics.TcpConnectionMs:F2} ms",
            Recommendation = tcpPassed ? null : "TCP connection time is high. Consider using CDNs or edge locations to reduce network latency.",
            Category = AuditCategory.Performance
        });

        // Server Response Time
        AuditStatus ttfbStatus;
        string? ttfbRecommendation = null;

        if (metrics.TtfbMs < 800)
        {
            ttfbStatus = AuditStatus.Passed;
        }
        else if (metrics.TtfbMs < 1800)
        {
            ttfbStatus = AuditStatus.Warning;
            ttfbRecommendation = "Initial server response time (TTFB) is high. Consider using CDN caching, optimizing database queries, or minifying application code.";
        }
        else
        {
            ttfbStatus = AuditStatus.Failed;
            ttfbRecommendation = "CRITICAL: Server response time (TTFB) is extremely poor (over 1.2s). This severely damages UX and Google crawling. Immediate server upgrade, hardware scaling, or aggressive object caching is required.";
        }

        audits.Add(new SeoAudit
        {
            Title = "Server Response Time (TTFB)",
            Status = ttfbStatus,
            Value = $"{metrics.TtfbMs:F2} ms",
            Recommendation = ttfbRecommendation,
            Category = AuditCategory.Performance
        });

        // Content Download
        var downloadPassed = metrics.ContentDownloadMs < 500;
        audits.Add(new SeoAudit
        {
            Title = "Content Download Time",
            Status = downloadPassed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = $"{metrics.ContentDownloadMs:F2} ms",
            Recommendation = downloadPassed ? null : "HTML content download time is high. Minify HTML, compress text payloads (GZip/Brotli), or optimize server bandwidth.",
            Category = AuditCategory.Performance
        });

        // Total Network Time
        var totalPassed = metrics.TotalNetworkTimeMs < 3000;
        audits.Add(new SeoAudit
        {
            Title = "Total Network Time",
            Status = totalPassed ? AuditStatus.Passed : AuditStatus.Warning,
            Value = $"{metrics.TotalNetworkTimeMs:F2} ms",
            Recommendation = totalPassed ? null : "Total network execution time is high. Address latency, connection setup, and server processing times.",
            Category = AuditCategory.Performance
        });

        return audits;
    }
}
