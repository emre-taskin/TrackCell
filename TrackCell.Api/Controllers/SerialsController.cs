using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("serial")]
    public class SerialsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public SerialsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("byPart/{partId}")]
        public async Task<IActionResult> GetSerialsByPart(int partId)
        {
            var data = await _dbContext.PartSerials
                .Where(s => s.PartDefinitionId == partId)
                .OrderBy(s => s.SerialNumber)
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("lookup/{serialNumber}")]
        public async Task<IActionResult> LookupSerial(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return BadRequest("SerialNumber is required.");

            var trimmed = serialNumber.Trim();

            // Use case-insensitive comparison for better user experience
            var serial = await _dbContext.PartSerials
                .Include(s => s.PartDefinition)
                .FirstOrDefaultAsync(s => s.SerialNumber.ToLower() == trimmed.ToLower());

            if (serial == null)
                return NotFound($"Serial '{trimmed}' was not found.");

            // Ensure PartDefinition is loaded (sometimes Include behaves weirdly with In-Memory)
            var part = serial.PartDefinition ?? await _dbContext.PartDefinitions.FindAsync(serial.PartDefinitionId);
            
            if (part == null)
                return NotFound($"Part definition for serial '{trimmed}' was not found (PartID: {serial.PartDefinitionId}).");

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

            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddSerial([FromBody] CreatePartSerialDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.SerialNumber))
                return BadRequest("SerialNumber is required.");

            var partExists = await _dbContext.PartDefinitions.AnyAsync(p => p.Id == dto.PartDefinitionId);
            if (!partExists)
                return NotFound($"Part with ID {dto.PartDefinitionId} not found.");

            var exists = await _dbContext.PartSerials.AnyAsync(s => 
                s.PartDefinitionId == dto.PartDefinitionId && 
                s.SerialNumber.ToLower() == dto.SerialNumber.Trim().ToLower());
            
            if (exists)
                return BadRequest($"Serial '{dto.SerialNumber}' already exists for this part.");

            var newSerial = new PartSerial
            {
                PartDefinitionId = dto.PartDefinitionId,
                SerialNumber = dto.SerialNumber.Trim()
            };

            _dbContext.PartSerials.Add(newSerial);
            await _dbContext.SaveChangesAsync();

            return Ok(newSerial);
        }
    }
}
