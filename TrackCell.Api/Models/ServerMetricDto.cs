using System;
using System.ComponentModel.DataAnnotations;

namespace TrackCell.Api.Models
{
    public class ServerMetricDto
    {
        [Required]
        [MaxLength(255)]
        public string MachineName { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public double CpuUsagePercent { get; set; }

        public long TotalMemoryBytes { get; set; }

        public long AvailableMemoryBytes { get; set; }

        public double MemoryUsagePercent { get; set; }

        public long TotalDiskBytes { get; set; }

        public long AvailableDiskBytes { get; set; }

        public double DiskUsagePercent { get; set; }

        public long UptimeSeconds { get; set; }

        [MaxLength(20)]
        public string HealthStatus { get; set; } = "Healthy";
    }
}
