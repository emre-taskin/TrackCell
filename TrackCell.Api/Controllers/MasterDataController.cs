using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Services;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly OperationHistoryService _historyService;

        public MasterDataController(AppDbContext dbContext, OperationHistoryService historyService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
        }

        [HttpGet("operators")]
        public async Task<IActionResult> GetOperators()
        {
            var data = await _dbContext.Operators.OrderBy(x => x.Name).ToListAsync();
            return Ok(data);
        }

        [HttpGet("parts")]
        public async Task<IActionResult> GetParts()
        {
            var data = await _dbContext.PartDefinitions.OrderBy(x => x.PartNumber).ToListAsync();
            return Ok(data);
        }

        [HttpGet("operations")]
        public async Task<IActionResult> GetOperations()
        {
            var data = await _dbContext.OperationDefinitions.OrderBy(x => x.OpNumber).ToListAsync();
            return Ok(data);
        }

        // Lookup a single operator by badge number (used by barcode scan on Operator Entry).
        [HttpGet("operators/{badgeNumber}")]
        public async Task<IActionResult> GetOperatorByBadge(string badgeNumber)
        {
            var op = await _dbContext.Operators
                .FirstOrDefaultAsync(x => x.BadgeNumber == badgeNumber);
            if (op == null) return NotFound();
            return Ok(op);
        }

        // Lookup a serial number in operation history.
        // Returns the part this serial belongs to, and the list of completed op numbers,
        // so the Operator Entry page can auto-fill the part and suggest the next op.
        [HttpGet("serial/{serialNumber}")]
        public async Task<IActionResult> GetSerialHistory(string serialNumber)
        {
            var history = await _historyService.GetSerialHistoryAsync(serialNumber);
            if (history == null) return NotFound();
            return Ok(history);
        }
    }
}
