using System.Threading.Channels;
using EdgeCollector.Data;
using EdgeCollector.Models;
using EdgeCollector.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "EdgeCollector";
});

builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<IMessageRepository, MessageRepository>();

builder.Services.AddSingleton(Channel.CreateBounded<MTConnectData>(new BoundedChannelOptions(1000)
{
    FullMode = BoundedChannelFullMode.Wait
}));

builder.Services.AddHostedService<MTConnectPollingWorker>();
builder.Services.AddHostedService<MTConnectAdapterWorker>();
builder.Services.AddHostedService<DatabaseWriterWorker>();
builder.Services.AddHostedService<RabbitMQDispatcherWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    initializer.Initialize();
}

host.Run();
