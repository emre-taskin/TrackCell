using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Application.Interfaces;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportingController : ControllerBase
    {
        private readonly IReportingService _service;

        public ReportingController(IReportingService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _service.GetDashboardSummaryAsync();
            return Ok(summary);
        }
    }
}
