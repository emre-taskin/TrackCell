namespace ServerMonitor.Models;

public class ServerMetricSnapshot
{
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
    public string HealthStatus { get; set; } = "Healthy";
}
