using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackCell.Domain.Entities;

namespace TrackCell.Infrastructure.Persistence.Configurations
{
    public class ServerMetricConfiguration : IEntityTypeConfiguration<ServerMetric>
    {
        public void Configure(EntityTypeBuilder<ServerMetric> builder)
        {
            builder.HasIndex(m => new { m.MachineName, m.Timestamp });
        }
    }
}
