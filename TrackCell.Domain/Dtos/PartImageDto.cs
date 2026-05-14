using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Dtos
{
    public class PartImageDto
    {
        public int Id { get; set; }
        public int PartDefinitionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public List<ImageZoneDto> Zones { get; set; } = new();
    }

    public class ImageZoneDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<int> NonConformanceIds { get; set; } = new();
    }

    public class SaveZonesRequest
    {
        public List<ZoneInput> Zones { get; set; } = new();
    }

    public class ZoneInput
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public List<int> NonConformanceIds { get; set; } = new();
    }
}
