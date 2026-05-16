using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class PartService : IPartService
    {
        private readonly ApplicationDbContext _dbContext;

        public PartService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<PartDefinition>> GetAllAsync()
        {
            return _dbContext.PartDefinitions.OrderBy(x => x.PartNumber).ToListAsync();
        }

        public async Task<(PartDefinition? Part, string? Error)> AddAsync(CreatePartDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PartNumber))
            {
                return (null, "PartNumber is required.");
            }

            var exists = await _dbContext.PartDefinitions.AnyAsync(p => p.PartNumber == dto.PartNumber);
            if (exists)
            {
                return (null, $"Part '{dto.PartNumber}' already exists.");
            }

            var newPart = new PartDefinition
            {
                PartNumber = dto.PartNumber.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty
            };

            _dbContext.PartDefinitions.Add(newPart);
            await _dbContext.SaveChangesAsync();

            return (newPart, null);
        }
    }
}
