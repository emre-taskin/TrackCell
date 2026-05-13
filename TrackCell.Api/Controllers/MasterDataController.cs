using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Services;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly OperationHistoryService _historyService;

        public MasterDataController(AppDbContext dbContext, OperationHistoryService historyService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
        }

        [HttpGet("getParts")]
        public async Task<IActionResult> GetParts()
        {
            var data = await _dbContext.PartDefinitions.OrderBy(x => x.PartNumber).ToListAsync();
            return Ok(data);
        }

        // No PartDefinition→OperationDefinition relationship exists in the schema,
        // so this returns the global operations list and validates the part exists.
        [HttpGet("getOperationsByPart")]
        public async Task<IActionResult> GetOperationsByPart([FromQuery] string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return BadRequest("partNumber is required.");

            var partExists = await _dbContext.PartDefinitions
                .AnyAsync(p => p.PartNumber == partNumber);
            if (!partExists) return NotFound($"Part '{partNumber}' not found.");

            var data = await _dbContext.OperationDefinitions
                .OrderBy(x => x.OpNumber)
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("getOperators")]
        public async Task<IActionResult> GetOperators()
        {
            var data = await _dbContext.Operators.OrderBy(x => x.Name).ToListAsync();
            return Ok(data);
        }

        [HttpGet("getOperations")]
        public async Task<IActionResult> GetOperations()
        {
            var data = await _dbContext.OperationDefinitions.OrderBy(x => x.OpNumber).ToListAsync();
            return Ok(data);
        }

        [HttpGet("getOperatorByBadge/{badgeNumber}")]
        public async Task<IActionResult> GetOperatorByBadge(string badgeNumber)
        {
            var op = await _dbContext.Operators
                .FirstOrDefaultAsync(x => x.BadgeNumber == badgeNumber);
            if (op == null) return NotFound();
            return Ok(op);
        }

        [HttpGet("getSerialHistory/{serialNumber}")]
        public async Task<IActionResult> GetSerialHistory(string serialNumber)
        {
            var history = await _historyService.GetSerialHistoryAsync(serialNumber);
            if (history == null) return NotFound();
            return Ok(history);
        }
    }
}
