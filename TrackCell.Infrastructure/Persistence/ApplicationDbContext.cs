using Microsoft.EntityFrameworkCore;
using TrackCell.Domain.Entities;

namespace TrackCell.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Operator> Operators { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<PartDefinition> PartDefinitions { get; set; } = null!;
        public DbSet<OperationDefinition> OperationDefinitions { get; set; } = null!;
        public DbSet<OperationHistory> OperationHistories { get; set; } = null!;
        public DbSet<ServerMetric> ServerMetrics { get; set; } = null!;
        public DbSet<NonConformance> NonConformances { get; set; } = null!;
        public DbSet<PartImage> PartImages { get; set; } = null!;
        public DbSet<ImageZone> ImageZones { get; set; } = null!;
        public DbSet<ImageZoneNonConformance> ImageZoneNonConformances { get; set; } = null!;
        public DbSet<InspectionResult> InspectionResults { get; set; } = null!;
        public DbSet<PartSerial> PartSerials { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<Operator>().HasData(
                new Operator { Id = 1, BadgeNumber = "EMP-1001", Name = "Alice Smith" },
                new Operator { Id = 2, BadgeNumber = "EMP-1002", Name = "Bob Johnson" },
                new Operator { Id = 3, BadgeNumber = "EMP-1003", Name = "Charlie Brown" },
                new Operator { Id = 4, BadgeNumber = "EMP-1004", Name = "Diana Prince" }
            );

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, WindowsAccount = "DOMAIN\\asmith",  DisplayName = "Alice Smith",   Role = "Operator", BadgeNumber = "EMP-1001" },
                new User { Id = 2, WindowsAccount = "DOMAIN\\bjohnson", DisplayName = "Bob Johnson",   Role = "Operator", BadgeNumber = "EMP-1002" },
                new User { Id = 3, WindowsAccount = "DOMAIN\\cbrown",  DisplayName = "Charlie Brown", Role = "Supervisor", BadgeNumber = "EMP-1003" },
                new User { Id = 4, WindowsAccount = "DOMAIN\\dprince", DisplayName = "Diana Prince",  Role = "Admin",      BadgeNumber = "EMP-1004" }
            );

            modelBuilder.Entity<PartDefinition>().HasData(
                new PartDefinition { Id = 1, PartNumber = "PRT-001X", Description = "Chassis Assembly" },
                new PartDefinition { Id = 2, PartNumber = "PRT-002Y", Description = "Engine Block" },
                new PartDefinition { Id = 3, PartNumber = "PRT-003Z", Description = "Transmission Unit" },
                new PartDefinition { Id = 4, PartNumber = "PRT-004A", Description = "Wiring Harness" }
            );

            modelBuilder.Entity<OperationDefinition>().HasData(
                new OperationDefinition { Id = 1, OpNumber = "OP-10", Description = "Machining", PartDefinitionId = 1 },
                new OperationDefinition { Id = 2, OpNumber = "OP-20", Description = "Sub-Assembly", PartDefinitionId = 1 },
                new OperationDefinition { Id = 3, OpNumber = "OP-30", Description = "Primary Assembly", PartDefinitionId = 1 },
                new OperationDefinition { Id = 4, OpNumber = "OP-40", Description = "Testing and QA", PartDefinitionId = 2 },
                new OperationDefinition { Id = 5, OpNumber = "OP-50", Description = "Packaging", PartDefinitionId = 2 }
            );

            modelBuilder.Entity<NonConformance>().HasData(
                new NonConformance { Id = 1, Code = "NC-SCR", Description = "Scratch" },
                new NonConformance { Id = 2, Code = "NC-DNT", Description = "Dent" },
                new NonConformance { Id = 3, Code = "NC-MIS", Description = "Missing Fastener" },
                new NonConformance { Id = 4, Code = "NC-ALN", Description = "Misalignment" },
                new NonConformance { Id = 5, Code = "NC-COR", Description = "Corrosion" },
                new NonConformance { Id = 6, Code = "NC-CLR", Description = "Wrong Color" },
                new NonConformance { Id = 7, Code = "NC-CRK", Description = "Crack" },
                new NonConformance { Id = 8, Code = "NC-BRR", Description = "Burr / Sharp Edge" }
            );

            modelBuilder.Entity<PartSerial>().HasData(
                new PartSerial { Id = 1, PartDefinitionId = 1, SerialNumber = "SN-001X-01" },
                new PartSerial { Id = 2, PartDefinitionId = 1, SerialNumber = "SN-001X-02" },
                new PartSerial { Id = 3, PartDefinitionId = 2, SerialNumber = "SN-002Y-01" }
            );
        }
    }
}
