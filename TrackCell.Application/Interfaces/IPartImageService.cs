using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Domain.Dtos;

namespace TrackCell.Application.Interfaces
{
    public interface IPartImageService
    {
        Task<List<PartImageDto>> ListAsync(int partDefinitionId);
        Task<PartImageDto?> GetByIdAsync(int id);
        Task<(PartImageDto? Image, string? Error)> UploadAsync(PartImageUploadInput input);
        Task<bool> DeleteAsync(int id);
        Task<PartImageDto?> SaveZonesAsync(int id, SaveZonesRequest request);
    }
}
