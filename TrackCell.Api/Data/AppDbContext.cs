using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Models;

namespace TrackCell.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Operator> Operators { get; set; } = null!;
        public DbSet<PartDefinition> PartDefinitions { get; set; } = null!;
        public DbSet<OperationDefinition> OperationDefinitions { get; set; } = null!;
        public DbSet<OperationHistory> OperationHistories { get; set; } = null!;
        public DbSet<ServerMetric> ServerMetrics { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServerMetric>(b =>
            {
                b.HasIndex(m => new { m.MachineName, m.Timestamp });
            });
            
            // Seed Dummy Data so the UI dropdowns have things to show!
            modelBuilder.Entity<Operator>().HasData(
                new Operator { Id = 1, BadgeNumber = "EMP-1001", Name = "Alice Smith" },
                new Operator { Id = 2, BadgeNumber = "EMP-1002", Name = "Bob Johnson" },
                new Operator { Id = 3, BadgeNumber = "EMP-1003", Name = "Charlie Brown" },
                new Operator { Id = 4, BadgeNumber = "EMP-1004", Name = "Diana Prince" }
            );

            modelBuilder.Entity<PartDefinition>().HasData(
                new PartDefinition { Id = 1, PartNumber = "PRT-001X", Description = "Chassis Assembly" },
                new PartDefinition { Id = 2, PartNumber = "PRT-002Y", Description = "Engine Block" },
                new PartDefinition { Id = 3, PartNumber = "PRT-003Z", Description = "Transmission Unit" },
                new PartDefinition { Id = 4, PartNumber = "PRT-004A", Description = "Wiring Harness" }
            );

            modelBuilder.Entity<OperationDefinition>().HasData(
                new OperationDefinition { Id = 1, OpNumber = "OP-10", Description = "Machining" },
                new OperationDefinition { Id = 2, OpNumber = "OP-20", Description = "Sub-Assembly" },
                new OperationDefinition { Id = 3, OpNumber = "OP-30", Description = "Primary Assembly" },
                new OperationDefinition { Id = 4, OpNumber = "OP-40", Description = "Testing and QA" },
                new OperationDefinition { Id = 5, OpNumber = "OP-50", Description = "Packaging" }
            );
        }
    }
}
