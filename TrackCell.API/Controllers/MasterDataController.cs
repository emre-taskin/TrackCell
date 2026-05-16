using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.API.Services;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly OperationHistoryService _historyService;

        public MasterDataController(OperationHistoryService historyService)
        {
            _historyService = historyService;
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
