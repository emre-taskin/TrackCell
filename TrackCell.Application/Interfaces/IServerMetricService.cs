using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;

namespace TrackCell.Application.Interfaces
{
    public interface IServerMetricService
    {
        Task<ServerMetric> RecordAsync(ServerMetricDto dto);
        Task<List<ServerMetric>> GetRecentAsync(string? machineName, int take);
        Task<ServerMetric?> GetLatestAsync(string machineName);
    }
}
