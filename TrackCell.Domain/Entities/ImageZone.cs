using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class ImageZone : IEntity
    {
        [Key]
        public int Id { get; set; }

        public int PartImageId { get; set; }
        public PartImage? PartImage { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public List<ImageZoneNonConformance> NonConformances { get; set; } = new();
    }
}
