using System.Text.Json;
using System.Threading.Channels;
using TrackCell.CollectorService.Data;
using TrackCell.CollectorService.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TrackCell.CollectorService.Services;

public class DatabaseWriterWorker : BackgroundService
{
    private readonly ILogger<DatabaseWriterWorker> _logger;
    private readonly ChannelReader<MTConnectData> _channelReader;
    private readonly IMessageRepository _repository;

    public DatabaseWriterWorker(
        ILogger<DatabaseWriterWorker> logger,
        Channel<MTConnectData> channel,
        IMessageRepository repository)
    {
        _logger = logger;
        _channelReader = channel.Reader;
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var data in _channelReader.ReadAllAsync(stoppingToken))
            {
                var payload = JsonSerializer.Serialize(data);
                await _repository.InsertMessageAsync(payload);
                _logger.LogDebug("Saved message to SQLite: {Payload}", payload);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DatabaseWriterWorker is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to database");
        }
    }
}
