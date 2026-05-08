using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace ServerMonitor.Services;

public class MetricsReporterWorker : BackgroundService
{
    private readonly ILogger<MetricsReporterWorker> _logger;
    private readonly IMetricsCollector _collector;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TimeSpan _interval;
    private readonly string _endpoint;
    private readonly AsyncRetryPolicy _retryPolicy;

    public MetricsReporterWorker(
        ILogger<MetricsReporterWorker> logger,
        IMetricsCollector collector,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _collector = collector;
        _httpClientFactory = httpClientFactory;

        var monitor = configuration.GetSection("Monitor");
        _interval = TimeSpan.FromSeconds(monitor.GetValue("SampleIntervalSeconds", 15));
        _endpoint = monitor.GetValue("ReportPath", "api/servermetrics")!;

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 4,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (ex, delay, attempt, _) =>
                    _logger.LogWarning(ex, "Failed to push metrics (attempt {Attempt}); retrying in {Delay}s", attempt, delay.TotalSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ServerMonitor started. Sampling every {Interval}s, posting to {Endpoint}",
            _interval.TotalSeconds, _endpoint);

        using var timer = new PeriodicTimer(_interval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var snapshot = _collector.Collect();
                _logger.LogInformation(
                    "Collected metrics for {Machine}: CPU {Cpu}%, Memory {Memory}%, Disk {Disk}%, Health {Health}",
                    snapshot.MachineName, snapshot.CpuUsagePercent, snapshot.MemoryUsagePercent,
                    snapshot.DiskUsagePercent, snapshot.HealthStatus);

                await _retryPolicy.ExecuteAsync(async ct =>
                {
                    var client = _httpClientFactory.CreateClient("TrackCellApi");
                    var response = await client.PostAsJsonAsync(_endpoint, snapshot, ct);
                    response.EnsureSuccessStatusCode();
                }, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to report metrics; will try again next interval.");
            }
        }
    }
}
