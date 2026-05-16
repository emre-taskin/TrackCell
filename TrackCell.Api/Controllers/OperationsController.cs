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
    [Route("operation")]
    public class OperationsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public OperationsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("byPart/{partId}")]
        public async Task<IActionResult> GetOperationsByPart(int partId)
        {
            var data = await _dbContext.OperationDefinitions
                .Where(o => o.PartDefinitionId == partId)
                .OrderBy(o => o.OpNumber)
                .ToListAsync();
            return Ok(data);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddOperation([FromBody] CreateOperationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.OpNumber))
            {
                return BadRequest("OpNumber is required.");
            }

            var partExists = await _dbContext.PartDefinitions.AnyAsync(p => p.Id == dto.PartDefinitionId);
            if (!partExists)
            {
                return NotFound($"Part with ID {dto.PartDefinitionId} not found.");
            }

            var exists = await _dbContext.OperationDefinitions.AnyAsync(o => o.PartDefinitionId == dto.PartDefinitionId && o.OpNumber == dto.OpNumber);
            if (exists)
            {
                return BadRequest($"Operation '{dto.OpNumber}' already exists for this part.");
            }

            var newOp = new OperationDefinition
            {
                PartDefinitionId = dto.PartDefinitionId,
                OpNumber = dto.OpNumber.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty
            };

            _dbContext.OperationDefinitions.Add(newOp);
            await _dbContext.SaveChangesAsync();

            return Ok(newOp);
        }
    }
}
