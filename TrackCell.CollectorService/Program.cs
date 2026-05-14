using System.Threading.Channels;
using TrackCell.CollectorService.Data;
using TrackCell.CollectorService.Models;
using TrackCell.CollectorService.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "TrackCell.CollectorService";
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
