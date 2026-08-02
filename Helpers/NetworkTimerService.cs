using SeoAnalyzer.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SeoAnalyzer.Helpers;

/// <summary>Fetches HTML pages and measures accurate network connection timings.</summary>
internal static class NetworkTimerService
{
    public static async Task<NetworkFetchResult?> FetchAndMeasureAsync(string url)
    {
        try
        {
            var metrics = new NetworkPerformanceMetrics();
            var total = System.Diagnostics.Stopwatch.StartNew();
            var cookieContainer = new CookieContainer();

            using var client = BuildHttpClient(metrics, cookieContainer);

            var ttfb = System.Diagnostics.Stopwatch.StartNew();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            ttfb.Stop();
            metrics.TtfbMs = ttfb.Elapsed.TotalMilliseconds;

            if (response == null || !response.IsSuccessStatusCode)
                return null;

            var html = await ReadContentAsync(response, metrics);
            total.Stop();

            metrics.TotalNetworkTimeMs = total.Elapsed.TotalMilliseconds;

            var finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;
            var cookies = SerializeCookies(cookieContainer, finalUrl);

            return new NetworkFetchResult(
                html,
                metrics,
                response.Headers,
                finalUrl,
                cookies);
        }
        catch
        {
            return null;
        }
    }

    private static HttpClient BuildHttpClient(NetworkPerformanceMetrics metrics, CookieContainer cookieContainer)
    {
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            UseCookies = true,
            CookieContainer = cookieContainer,
            ConnectCallback = (ctx, ct) => ConnectAsync(ctx, metrics, ct)
        };

        handler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;

        var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) };

        UrlHelper.AddHeaders(client);
        return client;
    }

    private static string? SerializeCookies(CookieContainer container, string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null;

        var cookies = container.GetCookies(uri);
        if (cookies.Count == 0)
            return null;

        return string.Join("; ", cookies.Cast<Cookie>().Select(c => $"{c.Name}={c.Value}"));
    }

    private static async ValueTask<Stream> ConnectAsync(
        SocketsHttpConnectionContext context,
        NetworkPerformanceMetrics metrics,
        CancellationToken ct)
    {
        var dns = System.Diagnostics.Stopwatch.StartNew();
        var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, ct);
        dns.Stop();
        metrics.DnsLookupMs = dns.Elapsed.TotalMilliseconds;

        var tcp = System.Diagnostics.Stopwatch.StartNew();
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(addresses, context.DnsEndPoint.Port, ct);
        tcp.Stop();
        metrics.TcpConnectionMs = tcp.Elapsed.TotalMilliseconds;

        return new NetworkStream(socket, ownsSocket: true);
    }

    private static async Task<string> ReadContentAsync(HttpResponseMessage response, NetworkPerformanceMetrics metrics)
    {
        var download = System.Diagnostics.Stopwatch.StartNew();
        var html = await response.Content.ReadAsStringAsync();
        download.Stop();
        metrics.ContentDownloadMs = download.Elapsed.TotalMilliseconds;
        return html;
    }
}