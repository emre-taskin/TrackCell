using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackCell.Domain.Entities;

namespace TrackCell.Infrastructure.Persistence.Configurations
{
    public class ImageZoneConfiguration : IEntityTypeConfiguration<ImageZone>
    {
        public void Configure(EntityTypeBuilder<ImageZone> builder)
        {
            builder.HasOne(z => z.PartImage)
                .WithMany(p => p.Zones)
                .HasForeignKey(z => z.PartImageId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(z => z.PartImageId);
        }
    }
}
