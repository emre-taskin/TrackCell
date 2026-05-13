using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Api.Models;
using TrackCell.Api.Services;

namespace TrackCell.Api.Controllers
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
                string.IsNullOrWhiteSpace(item.Part) ||
                string.IsNullOrWhiteSpace(item.Serial) ||
                string.IsNullOrWhiteSpace(item.OpNumber))
            {
                return BadRequest("All properties (BadgeNumber, Part, Serial, OpNumber) are required.");
            }

            var createdItem = await _workItemService.StartOperationAsync(item);
            return Ok(createdItem);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> Complete([FromBody] CompleteOperationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Part) ||
                string.IsNullOrWhiteSpace(request.Serial) ||
                string.IsNullOrWhiteSpace(request.OpNumber))
            {
                return BadRequest("Part, Serial, and OpNumber are required.");
            }

            var success = await _workItemService.CompleteOperationAsync(
                request.Part, request.Serial, request.OpNumber, request.BadgeNumber);
            if (success)
            {
                return Ok(new { Message = "Operation completed successfully." });
            }
            return NotFound("Active work item not found.");
        }
    }

    public class CompleteOperationRequest
    {
        public string Part { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
        public string OpNumber { get; set; } = string.Empty;
        public string BadgeNumber { get; set; } = string.Empty;
    }
}
