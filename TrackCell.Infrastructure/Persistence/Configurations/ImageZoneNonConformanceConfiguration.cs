using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackCell.Domain.Entities;

namespace TrackCell.Infrastructure.Persistence.Configurations
{
    public class ImageZoneNonConformanceConfiguration : IEntityTypeConfiguration<ImageZoneNonConformance>
    {
        public void Configure(EntityTypeBuilder<ImageZoneNonConformance> builder)
        {
            builder.HasKey(x => new { x.ImageZoneId, x.NonConformanceId });

            builder.HasOne(x => x.ImageZone)
                .WithMany(z => z.NonConformances)
                .HasForeignKey(x => x.ImageZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NonConformance)
                .WithMany()
                .HasForeignKey(x => x.NonConformanceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
