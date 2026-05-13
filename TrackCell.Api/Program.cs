using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using TrackCell.Api.Constants;
using TrackCell.Api.Data.Repositories;
using TrackCell.Api.Hubs;
using TrackCell.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Redis Multiplexer
// Reads from "ConnectionStrings:Redis" in appsettings.json
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

// Add services to the container.
builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS policy - Setup for SignalR
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

// Configure OpenAPI
builder.Services.AddOpenApi();

// Register DbContext
// Reads from "ConnectionStrings:DefaultConnection" in appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TrackCell.Api.Data.AppDbContext>(options =>
    options.UseInMemoryDatabase("trackcell"));

// Register the generic repository so any IBaseRepository<TEntity> can be injected
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// Register the WorkItem service as Scoped since DbContext is Scoped
builder.Services.AddScoped<WorkItemService>();
builder.Services.AddScoped<OperationHistoryService>();
builder.Services.AddScoped<ServerMetricService>();
builder.Services.AddScoped<IUserService, UserService>();

// Authorization policies. The named policies referenced from controllers live here
// so callers can swap in role/claim checks once an authentication scheme is added.
// Until then, policies pass through so the API stays callable during development.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policy.Name.AuthorizationRead, p => p.RequireAssertion(_ => true));
    options.AddPolicy(Policy.Name.AuthorizationWrite, p => p.RequireAssertion(_ => true));
});

var app = builder.Build();

// Apply any pending migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TrackCell.Api.Data.AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Serve uploaded part images from wwwroot/uploads
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
