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
    public class SerialService : ISerialService
    {
        private readonly ApplicationDbContext _dbContext;

        public SerialService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<PartSerial>> GetByPartAsync(int partId)
        {
            return _dbContext.PartSerials
                .Where(s => s.PartDefinitionId == partId)
                .OrderBy(s => s.SerialNumber)
                .ToListAsync();
        }

        public async Task<(SerialLookupDto? Result, string? Error)> LookupAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return (null, "SerialNumber is required.");

            var trimmed = serialNumber.Trim();

            var serial = await _dbContext.PartSerials
                .Include(s => s.PartDefinition)
                .FirstOrDefaultAsync(s => s.SerialNumber.ToLower() == trimmed.ToLower());

            if (serial == null)
                return (null, $"Serial '{trimmed}' was not found.");

            var part = serial.PartDefinition ?? await _dbContext.PartDefinitions.FindAsync(serial.PartDefinitionId);

            if (part == null)
                return (null, $"Part definition for serial '{trimmed}' was not found (PartID: {serial.PartDefinitionId}).");

            var operations = await _dbContext.OperationDefinitions
                .Where(o => o.PartDefinitionId == serial.PartDefinitionId)
                .OrderBy(o => o.OpNumber)
                .ToListAsync();

            var result = new SerialLookupDto
            {
                PartSerial = new PartSerial
                {
                    Id = serial.Id,
                    PartDefinitionId = serial.PartDefinitionId,
                    SerialNumber = serial.SerialNumber
                },
                PartDefinition = new PartDefinition
                {
                    Id = part.Id,
                    PartNumber = part.PartNumber,
                    Description = part.Description
                },
                Operations = operations
            };

            return (result, null);
        }

        public async Task<(PartSerial? Serial, string? Error)> AddAsync(CreatePartSerialDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SerialNumber))
                return (null, "SerialNumber is required.");

            var partExists = await _dbContext.PartDefinitions.AnyAsync(p => p.Id == dto.PartDefinitionId);
            if (!partExists)
                return (null, $"Part with ID {dto.PartDefinitionId} not found.");

            var exists = await _dbContext.PartSerials.AnyAsync(s =>
                s.PartDefinitionId == dto.PartDefinitionId &&
                s.SerialNumber.ToLower() == dto.SerialNumber.Trim().ToLower());

            if (exists)
                return (null, $"Serial '{dto.SerialNumber}' already exists for this part.");

            var newSerial = new PartSerial
            {
                PartDefinitionId = dto.PartDefinitionId,
                SerialNumber = dto.SerialNumber.Trim()
            };

            _dbContext.PartSerials.Add(newSerial);
            await _dbContext.SaveChangesAsync();

            return (newSerial, null);
        }
    }
}
