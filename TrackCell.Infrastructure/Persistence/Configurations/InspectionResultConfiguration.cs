using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackCell.Domain.Entities;

namespace TrackCell.Infrastructure.Persistence.Configurations
{
    public class InspectionResultConfiguration : IEntityTypeConfiguration<InspectionResult>
    {
        public void Configure(EntityTypeBuilder<InspectionResult> builder)
        {
            // Deletion cascades via ImageZone (which itself cascades from PartImage),
            // so use NoAction here to avoid a multiple-cascade-path conflict.
            builder.HasOne(r => r.PartImage)
                .WithMany()
                .HasForeignKey(r => r.PartImageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.ImageZone)
                .WithMany()
                .HasForeignKey(r => r.ImageZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.NonConformance)
                .WithMany()
                .HasForeignKey(r => r.NonConformanceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => r.PartImageId);
            builder.HasIndex(r => r.ImageZoneId);
            builder.HasIndex(r => r.NonConformanceId);
        }
    }
}
