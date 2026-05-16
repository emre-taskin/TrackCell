using System;
using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class InspectionResult : IEntity
    {
        [Key]
        public int Id { get; set; }

        public int PartImageId { get; set; }
        public PartImage? PartImage { get; set; }

        public int ImageZoneId { get; set; }
        public ImageZone? ImageZone { get; set; }

        public int NonConformanceId { get; set; }
        public NonConformance? NonConformance { get; set; }

        public int? PartSerialId { get; set; }
        
        public PartSerial? PartSerial { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime InspectedAt { get; set; } = DateTime.UtcNow;
    }
}
