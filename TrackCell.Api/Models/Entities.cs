using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackCell.Api.Models
{
    public class Operator : IEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string BadgeNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class PartDefinition : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }

    public class OperationDefinition : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string OpNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }

    public class OperationHistory : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BadgeNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string OpNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ActionLevel { get; set; } = string.Empty; // "Started" or "Completed"

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class NonConformance : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }

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

    public class ImageZone : IEntity
    {
        [Key]
        public int Id { get; set; }

        public int PartImageId { get; set; }
        public PartImage? PartImage { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // Normalized 0..1 coordinates so zones survive resizing the image element.
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public List<ImageZoneNonConformance> NonConformances { get; set; } = new();
    }

    public class ImageZoneNonConformance
    {
        public int ImageZoneId { get; set; }
        public ImageZone? ImageZone { get; set; }

        public int NonConformanceId { get; set; }
        public NonConformance? NonConformance { get; set; }
    }

    public class ServerMetric
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string MachineName { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public double CpuUsagePercent { get; set; }

        public long TotalMemoryBytes { get; set; }

        public long AvailableMemoryBytes { get; set; }

        public double MemoryUsagePercent { get; set; }

        public long TotalDiskBytes { get; set; }

        public long AvailableDiskBytes { get; set; }

        public double DiskUsagePercent { get; set; }

        public long UptimeSeconds { get; set; }

        [Required]
        [MaxLength(20)]
        public string HealthStatus { get; set; } = "Healthy";
    }
}
