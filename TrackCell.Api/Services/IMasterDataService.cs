using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Api.Models;

namespace TrackCell.Api.Services
{
    public interface IMasterDataService
    {
        // Operators
        Task<IEnumerable<Operator>> GetOperatorsAsync();
        Task<Operator?> GetOperatorByBadgeAsync(string badgeNumber);
        Task<Operator> CreateOperatorAsync(Operator op);
        Task<bool> UpdateOperatorAsync(Operator op);
        Task<bool> DeleteOperatorAsync(int id);

        // Parts
        Task<IEnumerable<PartDefinition>> GetPartsAsync();
        Task<PartDefinition?> GetPartByNumberAsync(string partNumber);
        Task<PartDefinition> CreatePartAsync(PartDefinition part);
        Task<bool> UpdatePartAsync(PartDefinition part);
        Task<bool> DeletePartAsync(int id);

        // Operations
        Task<IEnumerable<OperationDefinition>> GetOperationsAsync();
        Task<OperationDefinition?> GetOperationByNumberAsync(string opNumber);
        Task<OperationDefinition> CreateOperationAsync(OperationDefinition op);
        Task<bool> UpdateOperationAsync(OperationDefinition op);
        Task<bool> DeleteOperationAsync(int id);

        // Serial History
        Task<SerialHistoryDto?> GetSerialHistoryAsync(string serialNumber);
    }

    public class SerialHistoryDto
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string PartDescription { get; set; } = string.Empty;
        public List<string> CompletedOps { get; set; } = new();
        public List<string> InProcessOps { get; set; } = new();
    }
}
