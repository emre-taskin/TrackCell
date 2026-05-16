using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;

namespace TrackCell.Application.Interfaces
{
    public interface IPartService
    {
        Task<List<PartDefinition>> GetAllAsync();
        Task<(PartDefinition? Part, string? Error)> AddAsync(CreatePartDto dto);
    }
}
