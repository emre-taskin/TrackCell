using System.Threading.Tasks;
using TrackCell.Domain.Dtos;

namespace TrackCell.Application.Interfaces
{
    public interface IReportingService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }
}
