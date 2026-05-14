using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class PartImage : IEntity
    {
        [Key]
        public int Id { get; set; }

        public int PartDefinitionId { get; set; }
        public PartDefinition? PartDefinition { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public List<ImageZone> Zones { get; set; } = new();
    }
}
