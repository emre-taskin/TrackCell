using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MasterDataController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
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
            var history = await _dbContext.OperationHistories
                .Where(h => h.SerialNumber == serialNumber)
                .OrderBy(h => h.Timestamp)
                .ToListAsync();

            if (history.Count == 0) return NotFound();

            // Use the most recent record's part number.
            var partNumber = history.Last().PartNumber;

            var part = await _dbContext.PartDefinitions
                .FirstOrDefaultAsync(p => p.PartNumber == partNumber);

            // Completed ops = ops that have a "Completed" record for this serial.
            var completedOps = history
                .Where(h => h.ActionLevel == "Completed")
                .Select(h => h.OpNumber)
                .Distinct()
                .ToList();

            // Started ops still in process = Started without a matching Completed.
            var startedOps = history
                .Where(h => h.ActionLevel == "Started")
                .Select(h => h.OpNumber)
                .Distinct()
                .Where(op => !completedOps.Contains(op))
                .ToList();

            return Ok(new
            {
                SerialNumber = serialNumber,
                PartNumber = partNumber,
                PartDescription = part?.Description ?? "",
                CompletedOps = completedOps,
                InProcessOps = startedOps
            });
        }
    }
}
