using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Domain.Entities;

namespace TrackCell.Application.Interfaces
{
    public interface INonConformanceService
    {
        Task<List<NonConformance>> GetAllAsync();
        Task<(NonConformance? Item, string? Error, bool Conflict)> CreateAsync(NonConformance body);
    }
}
