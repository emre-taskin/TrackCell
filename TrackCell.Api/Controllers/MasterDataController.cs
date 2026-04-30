using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Api.Models;
using TrackCell.Api.Services;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly IMasterDataService _masterDataService;

        public MasterDataController(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        [HttpGet("operators")]
        public async Task<IActionResult> GetOperators()
        {
            var data = await _masterDataService.GetOperatorsAsync();
            return Ok(data);
        }

        [HttpPost("operators")]
        public async Task<IActionResult> CreateOperator([FromBody] Operator op)
        {
            var created = await _masterDataService.CreateOperatorAsync(op);
            return CreatedAtAction(nameof(GetOperatorByBadge), new { badgeNumber = created.BadgeNumber }, created);
        }

        [HttpPut("operators/{id}")]
        public async Task<IActionResult> UpdateOperator(int id, [FromBody] Operator op)
        {
            if (id != op.Id) return BadRequest();
            var success = await _masterDataService.UpdateOperatorAsync(op);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("operators/{id}")]
        public async Task<IActionResult> DeleteOperator(int id)
        {
            var success = await _masterDataService.DeleteOperatorAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("parts")]
        public async Task<IActionResult> GetParts()
        {
            var data = await _masterDataService.GetPartsAsync();
            return Ok(data);
        }

        [HttpPost("parts")]
        public async Task<IActionResult> CreatePart([FromBody] PartDefinition part)
        {
            var created = await _masterDataService.CreatePartAsync(part);
            return Ok(created);
        }

        [HttpPut("parts/{id}")]
        public async Task<IActionResult> UpdatePart(int id, [FromBody] PartDefinition part)
        {
            if (id != part.Id) return BadRequest();
            var success = await _masterDataService.UpdatePartAsync(part);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("parts/{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            var success = await _masterDataService.DeletePartAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("operations")]
        public async Task<IActionResult> GetOperations()
        {
            var data = await _masterDataService.GetOperationsAsync();
            return Ok(data);
        }

        [HttpPost("operations")]
        public async Task<IActionResult> CreateOperation([FromBody] OperationDefinition op)
        {
            var created = await _masterDataService.CreateOperationAsync(op);
            return Ok(created);
        }

        [HttpPut("operations/{id}")]
        public async Task<IActionResult> UpdateOperation(int id, [FromBody] OperationDefinition op)
        {
            if (id != op.Id) return BadRequest();
            var success = await _masterDataService.UpdateOperationAsync(op);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("operations/{id}")]
        public async Task<IActionResult> DeleteOperation(int id)
        {
            var success = await _masterDataService.DeleteOperationAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // Lookup a single operator by badge number (used by barcode scan on Operator Entry).
        [HttpGet("operators/{badgeNumber}")]
        public async Task<IActionResult> GetOperatorByBadge(string badgeNumber)
        {
            var op = await _masterDataService.GetOperatorByBadgeAsync(badgeNumber);
            if (op == null) return NotFound();
            return Ok(op);
        }

        // Lookup a serial number in operation history.
        [HttpGet("serial/{serialNumber}")]
        public async Task<IActionResult> GetSerialHistory(string serialNumber)
        {
            var history = await _masterDataService.GetSerialHistoryAsync(serialNumber);
            if (history == null) return NotFound();
            return Ok(history);
        }
    }
}
