using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using SeoAnalyzer.Models;

namespace SeoAnalyzer.Rules.Security;

/// <summary>Audits connection TLS version for deprecated protocols.</summary>
internal static class TlsVersionRules
{
    public static async Task<SeoAudit?> ExecuteAsync(string? requestUrl)
    {
        if (string.IsNullOrWhiteSpace(requestUrl))
            return null;

        string host;
        try
        {
            var uri = new Uri(requestUrl);
            if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                return null;
            host = uri.Host;
        }
        catch
        {
            return null;
        }

        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, 443);
            if (await Task.WhenAny(connectTask, Task.Delay(3000)) != connectTask)
            {
                return null;
            }

            using var sslStream = new SslStream(client.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) => true);
            await sslStream.AuthenticateAsClientAsync(host);

            var protocol = sslStream.SslProtocol;
            bool passed = protocol is SslProtocols.Tls12 or SslProtocols.Tls13;

            return new SeoAudit
            {
                Title = "TLS Protocol Version",
                Passed = passed,
                Value = $"Negotiated {protocol}.",
                Weight = 5,
                Recommendation = passed ? null : "Upgrade server configuration to support TLS 1.2 or TLS 1.3 and disable deprecated protocols (TLS 1.0, TLS 1.1, SSL v3, SSL v2).",
                Category = AuditCategory.Security
            };
        }
        catch
        {
            return null;
        }
    }
}
