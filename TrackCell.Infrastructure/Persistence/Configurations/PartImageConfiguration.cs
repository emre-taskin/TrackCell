using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackCell.Domain.Entities;

namespace TrackCell.Infrastructure.Persistence.Configurations
{
    public class PartImageConfiguration : IEntityTypeConfiguration<PartImage>
    {
        public void Configure(EntityTypeBuilder<PartImage> builder)
        {
            builder.HasOne(p => p.PartDefinition)
                .WithMany()
                .HasForeignKey(p => p.PartDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(p => p.PartDefinitionId);
        }
    }
}
