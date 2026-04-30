using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using TrackCell.Api.Data;

namespace TrackCell.Api.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // We use a dummy connection string because EF Core just needs to know the provider
            // to generate migrations. The actual database update will use the real one.
            optionsBuilder.UseNpgsql("Host=localhost;Database=dummy;Username=dummy;Password=dummy")
                          .UseSnakeCaseNamingConvention();

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
