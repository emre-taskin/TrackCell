using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Models;
using TrackCell.Api.Services;
using System.Collections.Generic;

namespace TrackCell.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly MasterDataService _masterDataService;
        private readonly OperationHistoryService _historyService;

        public MasterDataController(AppDbContext dbContext, MasterDataService masterDataService, OperationHistoryService historyService)
        {
            _dbContext = dbContext;
            _masterDataService = masterDataService;
            _historyService = historyService;
        }

        [HttpGet("operators")]
        public async Task<IActionResult> GetOperators()
        {
            var data = await _dbContext.Operators.OrderBy(x => x.Name)
                .Select(x => new OperatorDto { BadgeNumber = x.BadgeNumber, Name = x.Name })
                .ToListAsync();
            var result = new ResultDto<IEnumerable<OperatorDto>> { Data = data };
            return Ok(result);
        }

        [HttpGet("parts")]
        public async Task<IActionResult> GetParts()
        {
            var data = await _dbContext.PartDefinitions.OrderBy(x => x.PartNumber)
                .Select(x => new PartDefinitionDto { PartNumber = x.PartNumber, Description = x.Description })
                .ToListAsync();
            var result = new ResultDto<IEnumerable<PartDefinitionDto>> { Data = data };
            return Ok(result);
        }

        [HttpGet("operations")]
        public async Task<IActionResult> GetOperations()
        {
            var data = await _dbContext.OperationDefinitions.OrderBy(x => x.OpNumber)
                .Select(x => new OperationDefinitionDto { PartNumber = x.PartNumber, OpNumber = x.OpNumber, Description = x.Description })
                .ToListAsync();
            var result = new ResultDto<IEnumerable<OperationDefinitionDto>> { Data = data };
            return Ok(result);
        }

        [HttpGet("getOperationsByPart")]
        public async Task<IActionResult> GetOperationsByPart([FromQuery] string partNumber)
        {
            var operations = await _masterDataService.GetOperationsByPartAsync(partNumber);
            var result = new ResultDto<IEnumerable<OperationDefinitionDto>>
            {
                Data = operations
            };
            return Ok(result);
        }

        // Lookup a single operator by badge number (used by barcode scan on Operator Entry).
        [HttpGet("operators/{badgeNumber}")]
        public async Task<IActionResult> GetOperatorByBadge(string badgeNumber)
        {
            var op = await _dbContext.Operators
                .FirstOrDefaultAsync(x => x.BadgeNumber == badgeNumber);
            if (op == null) return NotFound();

            var result = new ResultDto<OperatorDto>
            {
                Data = new OperatorDto { BadgeNumber = op.BadgeNumber, Name = op.Name }
            };
            return Ok(result);
        }

        // Lookup a serial number in operation history.
        // Returns the part this serial belongs to, and the list of completed op numbers,
        // so the Operator Entry page can auto-fill the part and suggest the next op.
        [HttpGet("serial/{serialNumber}")]
        public async Task<IActionResult> GetSerialHistory(string serialNumber)
        {
            var history = await _historyService.GetSerialHistoryAsync(serialNumber);
            if (history == null) return NotFound();

            var result = new ResultDto<SerialHistoryDto>
            {
                Data = history
            };
            return Ok(result);
        }
    }
}
