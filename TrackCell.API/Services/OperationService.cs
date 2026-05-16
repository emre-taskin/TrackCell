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
    public class OperationService : IOperationService
    {
        private readonly ApplicationDbContext _dbContext;

        public OperationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<OperationDefinition>> GetByPartAsync(int partId)
        {
            return _dbContext.OperationDefinitions
                .Where(o => o.PartDefinitionId == partId)
                .OrderBy(o => o.OpNumber)
                .ToListAsync();
        }

        public async Task<(OperationDefinition? Operation, string? Error)> AddAsync(CreateOperationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OpNumber))
            {
                return (null, "OpNumber is required.");
            }

            var partExists = await _dbContext.PartDefinitions.AnyAsync(p => p.Id == dto.PartDefinitionId);
            if (!partExists)
            {
                return (null, $"Part with ID {dto.PartDefinitionId} not found.");
            }

            var exists = await _dbContext.OperationDefinitions.AnyAsync(o =>
                o.PartDefinitionId == dto.PartDefinitionId && o.OpNumber == dto.OpNumber);
            if (exists)
            {
                return (null, $"Operation '{dto.OpNumber}' already exists for this part.");
            }

            var newOp = new OperationDefinition
            {
                PartDefinitionId = dto.PartDefinitionId,
                OpNumber = dto.OpNumber.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty
            };

            _dbContext.OperationDefinitions.Add(newOp);
            await _dbContext.SaveChangesAsync();

            return (newOp, null);
        }
    }
}
