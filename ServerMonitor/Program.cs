using ServerMonitor.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "TrackCell.ServerMonitor";
});

var apiBaseUrl = builder.Configuration.GetValue<string>("TrackCellApi:BaseUrl")
    ?? "http://localhost:5000/";

builder.Services.AddHttpClient("TrackCellApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

if (OperatingSystem.IsWindows())
{
    builder.Services.AddSingleton<IMetricsCollector, WindowsMetricsCollector>();
}
else
{
    throw new PlatformNotSupportedException(
        "ServerMonitor is designed to run as a Windows service and currently only supports Windows hosts.");
}

builder.Services.AddHostedService<MetricsReporterWorker>();

var host = builder.Build();
host.Run();
