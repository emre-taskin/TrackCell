using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using EdgeCollector.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EdgeCollector.Services;

public class MTConnectAdapterWorker : BackgroundService
{
    private readonly ILogger<MTConnectAdapterWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly ChannelWriter<MTConnectData> _channelWriter;

    public MTConnectAdapterWorker(
        ILogger<MTConnectAdapterWorker> logger,
        IConfiguration configuration,
        Channel<MTConnectData> channel)
    {
        _logger = logger;
        _configuration = configuration;
        _channelWriter = channel.Writer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var machines = _configuration.GetSection("MTConnect:Machines").Get<List<MachineConfig>>() ?? [];

        var tcpMachines = machines.Where(m => m.CommunicationType.Equals("TcpAdapter", StringComparison.OrdinalIgnoreCase)).ToList();

        if (tcpMachines.Count == 0)
        {
            _logger.LogInformation("No machines configured for TcpAdapter MTConnect adapter streams.");
            return;
        }

        var tasks = tcpMachines.Select(machine => ProcessMachineAdapterStreamAsync(machine, stoppingToken)).ToArray();

        await Task.WhenAll(tasks);
    }

    private async Task ProcessMachineAdapterStreamAsync(MachineConfig machine, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var uri = new Uri(machine.Url);
                string host = uri.Host;
                int port = uri.Port;

                _logger.LogInformation("Connecting to MTConnect adapter for machine {MachineName} at {Host}:{Port}", machine.Name, host, port);

                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(host, port, stoppingToken);

                using var networkStream = tcpClient.GetStream();
                using var reader = new StreamReader(networkStream, Encoding.UTF8);

                _logger.LogInformation("Connected to adapter for {MachineName}. Reading stream...", machine.Name);

                while (!stoppingToken.IsCancellationRequested && tcpClient.Connected)
                {
                    var line = await reader.ReadLineAsync(stoppingToken);
                    if (line == null)
                    {
                        break; // Connection closed
                    }

                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("*"))
                    {
                        // Ignore empty lines and commands (starting with *)
                        continue;
                    }

                    ParseAndWriteAdapterLine(machine.Name, line);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing adapter stream for machine {MachineName}. Reconnecting in 5 seconds...", machine.Name);
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private void ParseAndWriteAdapterLine(string machineName, string line)
    {
        // Example format: 2009-06-15T00:00:00.000000|power|ON|execution|ACTIVE|line|412|Xact|-1.1761875153|Yact|1766618937
        // Condition format: <timestamp>|<data_item_name>|<level>|<native_code>|<native_severity>|<qualifier>|<message>

        var parts = line.Split('|');
        if (parts.Length < 3)
        {
            return;
        }

        string timestamp = parts[0];

        // Process key-value pairs
        for (int i = 1; i < parts.Length - 1; i += 2)
        {
            string key = parts[i];
            string value = parts[i + 1];

            // In a real implementation we would determine TagType etc. from the data
            // For now we map it as an adapter stream value.

            var data = new MTConnectData
            {
                MachineName = machineName,
                DataItemId = key,
                Timestamp = timestamp,
                Value = value,
                Sequence = string.Empty, // Adapter streams don't typically have a sequence per line like XML
                TagType = "AdapterStream",
                Type = string.Empty,
                NativeCode = string.Empty,
                NativeSeverity = string.Empty,
                ConditionState = string.Empty
            };

            // Non-blocking write or could be async, but parsing line by line synchronously here
            _channelWriter.TryWrite(data);
        }
    }
}
