using System.Threading.Channels;
using System.Xml;
using EdgeCollector.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EdgeCollector.Services;

public class MTConnectPollingWorker : BackgroundService
{
    private readonly ILogger<MTConnectPollingWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly ChannelWriter<MTConnectData> _channelWriter;
    private readonly HttpClient _httpClient = new();

    public MTConnectPollingWorker(
        ILogger<MTConnectPollingWorker> logger,
        IConfiguration configuration,
        Channel<MTConnectData> channel)
    {
        _logger = logger;
        _configuration = configuration;
        _channelWriter = channel.Writer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollIntervalSeconds = _configuration.GetValue<int>("MTConnect:PollIntervalSeconds", 10);
        var machines = _configuration.GetSection("MTConnect:Machines").Get<List<MachineConfig>>() ?? [];

        if (machines.Count == 0)
        {
            _logger.LogWarning("No machines configured for MTConnect polling.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Polling MTConnect agents at: {time}", DateTimeOffset.Now);

            foreach (var machine in machines)
            {
                try
                {
                    _logger.LogInformation("Polling machine {MachineName} at {Url}", machine.Name, machine.Url);
                    var responseStream = await _httpClient.GetStreamAsync(machine.Url, stoppingToken);

                    using var xmlReader = XmlReader.Create(responseStream, new XmlReaderSettings { Async = true });

                    string currentTagType = string.Empty;

                    while (await xmlReader.ReadAsync())
                    {
                        if (xmlReader.NodeType == XmlNodeType.Element)
                        {
                            string elementName = xmlReader.LocalName;
                            if (elementName == "Events" || elementName == "Condition" || elementName == "Samples")
                            {
                                currentTagType = elementName;
                            }

                            if (xmlReader.HasAttributes)
                            {
                                var dataItemId = xmlReader.GetAttribute("dataItemId");

                                if (!string.IsNullOrEmpty(dataItemId))
                                {
                                    var sequence = xmlReader.GetAttribute("sequence") ?? string.Empty;
                                    var timestamp = xmlReader.GetAttribute("timestamp") ?? string.Empty;
                                    var type = xmlReader.GetAttribute("type") ?? string.Empty;
                                    var nativeCode = xmlReader.GetAttribute("nativeCode") ?? string.Empty;
                                    var nativeSeverity = xmlReader.GetAttribute("nativeSeverity") ?? string.Empty;

                                    bool isEmpty = xmlReader.IsEmptyElement;
                                    string value = string.Empty;

                                    if (!isEmpty)
                                    {
                                        await xmlReader.ReadAsync();
                                        if (xmlReader.NodeType == XmlNodeType.Text)
                                        {
                                            value = xmlReader.Value;
                                        }
                                    }

                                    var data = new MTConnectData
                                    {
                                        MachineName = machine.Name,
                                        DataItemId = dataItemId,
                                        Sequence = sequence,
                                        Timestamp = timestamp,
                                        Value = value,
                                        TagType = currentTagType,
                                        Type = type,
                                        NativeCode = nativeCode,
                                        NativeSeverity = nativeSeverity,
                                        ConditionState = currentTagType == "Condition" ? elementName : string.Empty
                                    };

                                    await _channelWriter.WriteAsync(data, stoppingToken);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error polling machine {MachineName} at {Url}", machine.Name, machine.Url);
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
