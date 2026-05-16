using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Domain.Entities;

namespace TrackCell.Application.Interfaces
{
    public interface IOperationHistoryService
    {
        Task<OperationHistory> StartOperationAsync(OperationHistory item);
        Task<bool> CompleteOperationAsync(int partSerialId, string opNumber, string badgeNumber);
        Task<IEnumerable<OperationHistory>> GetActiveOperationHistoriesAsync();
    }
}
