using Microsoft.Extensions.Configuration;

namespace TrackCell.Worker;

public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    private readonly HttpClient _httpClient = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollIntervalSeconds = configuration.GetValue<int>("MTConnect:PollIntervalSeconds", 10);
        var machines = configuration.GetSection("MTConnect:Machines").Get<List<MachineConfig>>() ?? [];

        if (machines.Count == 0)
        {
            logger.LogWarning("No machines configured for MTConnect polling.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker polling MTConnect agents at: {time}", DateTimeOffset.Now);

            foreach (var machine in machines)
            {
                try
                {
                    logger.LogInformation("Polling machine {MachineName} at {Url}", machine.Name, machine.Url);
                    var response = await _httpClient.GetStringAsync(machine.Url, stoppingToken);

                    // TODO: Parse MTConnect XML and send to TrackCell API
                    logger.LogDebug("Received response from {MachineName}: {ResponseLength} characters", machine.Name, response.Length);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error polling machine {MachineName} at {Url}", machine.Name, machine.Url);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds), stoppingToken);
        }
    }

    public override void Dispose()
    {
        _httpClient.Dispose();
        base.Dispose();
    }
}

public class MachineConfig
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
