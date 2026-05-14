using System;
using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
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
