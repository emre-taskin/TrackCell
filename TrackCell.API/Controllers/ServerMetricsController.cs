using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServerMetricsController : ControllerBase
    {
        private readonly IServerMetricService _service;

        public ServerMetricsController(IServerMetricService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [HttpPost]
        public async Task<IActionResult> Ingest([FromBody] ServerMetricDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.MachineName))
            {
                return BadRequest("MachineName is required.");
            }

            var saved = await _service.RecordAsync(dto);
            return CreatedAtAction(nameof(GetLatest), new { machineName = saved.MachineName }, saved);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent([FromQuery] string? machineName, [FromQuery] int take = 100)
        {
            var items = await _service.GetRecentAsync(machineName, take);
            return Ok(items);
        }

        [HttpGet("latest/{machineName}")]
        public async Task<IActionResult> GetLatest(string machineName)
        {
            var latest = await _service.GetLatestAsync(machineName);
            if (latest is null) return NotFound();
            return Ok(latest);
        }
    }
}
