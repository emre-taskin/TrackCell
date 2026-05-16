using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.API.Services;
using TrackCell.Domain.Entities;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OperationHistoryController : ControllerBase
    {
        private readonly WorkItemService _workItemService;

        public OperationHistoryController(WorkItemService workItemService)
        {
            _workItemService = workItemService;
        }

        [HttpGet("inprogress")]
        public async Task<IActionResult> InProgress()
        {
            var items = await _workItemService.GetActiveWorkItemsAsync();
            return Ok(items);
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] WorkItem item)
        {
            if (string.IsNullOrWhiteSpace(item.BadgeNumber) ||
                item.PartSerialId <= 0 ||
                string.IsNullOrWhiteSpace(item.OpNumber))
            {
                return BadRequest("All properties (BadgeNumber, PartSerialId, OpNumber) are required.");
            }

            var createdItem = await _workItemService.StartOperationAsync(item);
            return Ok(createdItem);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> Complete([FromBody] CompleteOperationRequest request)
        {
            if (request.PartSerialId <= 0 ||
                string.IsNullOrWhiteSpace(request.OpNumber))
            {
                return BadRequest("PartSerialId and OpNumber are required.");
            }

            var success = await _workItemService.CompleteOperationAsync(
                request.PartSerialId, request.OpNumber, request.BadgeNumber);
            if (success)
            {
                return Ok(new { Message = "Operation completed successfully." });
            }
            return NotFound("Active work item not found.");
        }
    }

    public class CompleteOperationRequest
    {
        public int PartSerialId { get; set; }
        public string OpNumber { get; set; } = string.Empty;
        public string BadgeNumber { get; set; } = string.Empty;
    }
}
