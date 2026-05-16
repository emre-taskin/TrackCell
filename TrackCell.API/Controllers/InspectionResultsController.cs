using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InspectionResultsController : ControllerBase
    {
        private readonly IInspectionResultService _service;

        public InspectionResultsController(IInspectionResultService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int partImageId)
        {
            if (partImageId <= 0) return BadRequest("partImageId is required.");
            var results = await _service.ListAsync(partImageId);
            return Ok(results);
        }

        [HttpGet("heatmap")]
        public async Task<IActionResult> Heatmap(
            [FromQuery] int partImageId,
            [FromQuery] int? nonConformanceId)
        {
            if (partImageId <= 0) return BadRequest("partImageId is required.");

            var response = await _service.GetHeatmapAsync(partImageId, nonConformanceId);
            if (response == null) return NotFound();
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInspectionResultRequest body)
        {
            if (body == null) return BadRequest();

            var (result, error) = await _service.CreateAsync(body);
            if (error != null) return BadRequest(error);
            return Ok(result);
        }
    }
}
