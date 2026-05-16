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
    [Route("part")]
    public class PartsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public PartsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetParts()
        {
            var data = await _dbContext.PartDefinitions.OrderBy(x => x.PartNumber).ToListAsync();
            return Ok(data);
        }

        [HttpPost("addPart")]
        public async Task<IActionResult> AddPart([FromBody] CreatePartDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.PartNumber))
            {
                return BadRequest("PartNumber is required.");
            }

            var exists = await _dbContext.PartDefinitions.AnyAsync(p => p.PartNumber == dto.PartNumber);
            if (exists)
            {
                return BadRequest($"Part '{dto.PartNumber}' already exists.");
            }

            var newPart = new PartDefinition
            {
                PartNumber = dto.PartNumber.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty
            };

            _dbContext.PartDefinitions.Add(newPart);
            await _dbContext.SaveChangesAsync();

            return Ok(newPart);
        }
    }
}
