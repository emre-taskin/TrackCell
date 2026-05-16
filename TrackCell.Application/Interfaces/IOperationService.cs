using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;

namespace TrackCell.Application.Interfaces
{
    public interface IOperationService
    {
        Task<List<OperationDefinition>> GetByPartAsync(int partId);
        Task<(OperationDefinition? Operation, string? Error)> AddAsync(CreateOperationDto dto);
    }
}
