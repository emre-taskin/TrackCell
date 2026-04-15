using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Api.Models;
using TrackCell.Api.Services;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkItemsController : ControllerBase
    {
        private readonly WorkItemService _workItemService;

        public WorkItemsController(WorkItemService workItemService)
        {
            _workItemService = workItemService;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveWorkItems()
        {
            var items = await _workItemService.GetActiveWorkItemsAsync();
            return Ok(items);
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartOperation([FromBody] WorkItem item)
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
        public async Task<IActionResult> CompleteOperation([FromBody] CompleteOperationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Part) ||
                string.IsNullOrWhiteSpace(request.Serial) ||
                string.IsNullOrWhiteSpace(request.OpNumber))
            {
                return BadRequest("Part, Serial, and OpNumber are required.");
            }

            var success = await _workItemService.CompleteOperationAsync(request.Part, request.Serial, request.OpNumber, request.BadgeNumber);
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
