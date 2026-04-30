using TrackCell.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "TrackCell.Worker";
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
