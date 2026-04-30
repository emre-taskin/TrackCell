using System.Text;
using EdgeCollector.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EdgeCollector.Services;

public class RabbitMQDispatcherWorker : BackgroundService
{
    private readonly ILogger<RabbitMQDispatcherWorker> _logger;
    private readonly IMessageRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ResiliencePipeline _resiliencePipeline;

    public RabbitMQDispatcherWorker(
        ILogger<RabbitMQDispatcherWorker> logger,
        IMessageRepository repository,
        IConfiguration configuration)
    {
        _logger = logger;
        _repository = repository;
        _configuration = configuration;

        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<RabbitMQClientException>()
            })
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<RabbitMQClientException>()
            })
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration.GetValue<string>("RabbitMQ:HostName", "localhost")!,
            Port = _configuration.GetValue<int>("RabbitMQ:Port", 5672)
        };
        var exchangeName = _configuration.GetValue<string>("RabbitMQ:Exchange", "mtconnect_topic")!;
        var routingKey = _configuration.GetValue<string>("RabbitMQ:RoutingKey", "mtconnect.data")!;

        IConnection? connection = null;
        IModel? channel = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _resiliencePipeline.ExecuteAsync(async ct =>
                {
                    if (connection == null || !connection.IsOpen)
                    {
                        connection = factory.CreateConnection();
                        channel = connection.CreateModel();
                        channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
                        channel.ConfirmSelect();
                    }
                    await Task.CompletedTask;
                }, stoppingToken);

                var message = await _repository.GetNextMessageAsync();
                if (message == null)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                await _resiliencePipeline.ExecuteAsync(async ct =>
                {
                    if (channel != null && channel.IsOpen)
                    {
                        var body = Encoding.UTF8.GetBytes(message.Value.Payload);
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, mandatory: true, basicProperties: properties, body: body);
                        channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

                        await _repository.DeleteMessageAsync(message.Value.Id);
                        _logger.LogDebug("Published and deleted message Id: {Id}", message.Value.Id);
                    }
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect or publish to RabbitMQ.");

                if (channel != null)
                {
                    channel.Dispose();
                    channel = null;
                }
                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        channel?.Dispose();
        connection?.Dispose();
    }
}
