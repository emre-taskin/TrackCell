using ServerMonitor.Models;

namespace ServerMonitor.Services;

public interface IMetricsCollector
{
    ServerMetricSnapshot Collect();
}
