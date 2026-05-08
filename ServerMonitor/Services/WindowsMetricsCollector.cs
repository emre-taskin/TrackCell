using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerMonitor.Models;

namespace ServerMonitor.Services;

[SupportedOSPlatform("windows")]
public class WindowsMetricsCollector : IMetricsCollector, IDisposable
{
    private readonly ILogger<WindowsMetricsCollector> _logger;
    private readonly PerformanceCounter _cpuCounter;
    private readonly double _cpuWarning;
    private readonly double _cpuCritical;
    private readonly double _memoryWarning;
    private readonly double _memoryCritical;
    private readonly double _diskWarning;
    private readonly double _diskCritical;
    private readonly string _monitoredDriveRoot;

    public WindowsMetricsCollector(ILogger<WindowsMetricsCollector> logger, IConfiguration configuration)
    {
        _logger = logger;
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        // Prime the counter so the first read returns a meaningful value.
        _ = _cpuCounter.NextValue();

        var thresholds = configuration.GetSection("Thresholds");
        _cpuWarning = thresholds.GetValue("CpuWarningPercent", 75d);
        _cpuCritical = thresholds.GetValue("CpuCriticalPercent", 90d);
        _memoryWarning = thresholds.GetValue("MemoryWarningPercent", 80d);
        _memoryCritical = thresholds.GetValue("MemoryCriticalPercent", 92d);
        _diskWarning = thresholds.GetValue("DiskWarningPercent", 85d);
        _diskCritical = thresholds.GetValue("DiskCriticalPercent", 95d);
        _monitoredDriveRoot = configuration.GetValue("Monitor:DriveRoot", "C:\\")!;
    }

    public ServerMetricSnapshot Collect()
    {
        var cpu = Math.Round(_cpuCounter.NextValue(), 2);

        var memStatus = new MEMORYSTATUSEX();
        long totalMemory = 0;
        long availableMemory = 0;
        if (GlobalMemoryStatusEx(memStatus))
        {
            totalMemory = (long)memStatus.ullTotalPhys;
            availableMemory = (long)memStatus.ullAvailPhys;
        }
        else
        {
            _logger.LogWarning("GlobalMemoryStatusEx failed; reporting zeros for memory.");
        }

        var memoryUsedPercent = totalMemory > 0
            ? Math.Round(100.0 * (totalMemory - availableMemory) / totalMemory, 2)
            : 0d;

        long totalDisk = 0;
        long availableDisk = 0;
        try
        {
            var drive = new DriveInfo(_monitoredDriveRoot);
            if (drive.IsReady)
            {
                totalDisk = drive.TotalSize;
                availableDisk = drive.AvailableFreeSpace;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read disk info for {Drive}", _monitoredDriveRoot);
        }

        var diskUsedPercent = totalDisk > 0
            ? Math.Round(100.0 * (totalDisk - availableDisk) / totalDisk, 2)
            : 0d;

        var uptimeSeconds = Environment.TickCount64 / 1000;

        return new ServerMetricSnapshot
        {
            MachineName = Environment.MachineName,
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = cpu,
            TotalMemoryBytes = totalMemory,
            AvailableMemoryBytes = availableMemory,
            MemoryUsagePercent = memoryUsedPercent,
            TotalDiskBytes = totalDisk,
            AvailableDiskBytes = availableDisk,
            DiskUsagePercent = diskUsedPercent,
            UptimeSeconds = uptimeSeconds,
            HealthStatus = DeriveHealth(cpu, memoryUsedPercent, diskUsedPercent)
        };
    }

    private string DeriveHealth(double cpu, double memory, double disk)
    {
        if (cpu >= _cpuCritical || memory >= _memoryCritical || disk >= _diskCritical)
            return "Critical";
        if (cpu >= _cpuWarning || memory >= _memoryWarning || disk >= _diskWarning)
            return "Warning";
        return "Healthy";
    }

    public void Dispose() => _cpuCounter.Dispose();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }
}
