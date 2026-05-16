using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.API.Services;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OperationHistoryService _historyService;

        public MasterDataController(ApplicationDbContext dbContext, OperationHistoryService historyService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
        }


        [HttpGet("getOperators")]
        public async Task<IActionResult> GetOperators()
        {
            var data = await _dbContext.Operators.OrderBy(x => x.Name).ToListAsync();
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
