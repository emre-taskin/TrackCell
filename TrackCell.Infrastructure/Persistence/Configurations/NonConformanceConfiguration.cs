using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackCell.Domain.Entities;

namespace TrackCell.Infrastructure.Persistence.Configurations
{
    public class NonConformanceConfiguration : IEntityTypeConfiguration<NonConformance>
    {
        public void Configure(EntityTypeBuilder<NonConformance> builder)
        {
            builder.HasIndex(n => n.Code).IsUnique();
        }
    }
}
