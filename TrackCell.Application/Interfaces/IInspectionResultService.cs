using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Domain.Dtos;

namespace TrackCell.Application.Interfaces
{
    public interface IInspectionResultService
    {
        Task<List<InspectionResultDto>> ListAsync(int partImageId);
        Task<HeatmapResponseDto?> GetHeatmapAsync(int partImageId, int? nonConformanceId);
        Task<(InspectionResultDto? Result, string? Error)> CreateAsync(CreateInspectionResultRequest body);
    }
}
