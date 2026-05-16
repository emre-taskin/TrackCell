using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TrackCell.API.Hubs;
using TrackCell.API.Services;
using TrackCell.API.Utils;
using TrackCell.Application.Interfaces;
using TrackCell.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetConnectionString(Constants.RedisConnectionStringName);
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString(Constants.DefaultConnectionStringName);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase(Constants.InMemoryDatabaseName));

builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<OperationHistoryService>();
builder.Services.AddScoped<ServerMetricService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policy.Name.AuthorizationRead, p => p.RequireAssertion(_ => true));
    options.AddPolicy(Policy.Name.AuthorizationWrite, p => p.RequireAssertion(_ => true));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

{
    var webRoot = app.Environment.WebRootPath;
    if (string.IsNullOrEmpty(webRoot))
    {
        webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
    }
    Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "parts"));
}
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<DashboardHub>("/dashboardHub");

app.Run();
