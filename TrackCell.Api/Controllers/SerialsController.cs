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
                s.SerialNumber == dto.SerialNumber);
            
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
