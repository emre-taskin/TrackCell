using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportingController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportingController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var today = DateTime.UtcNow.Date;

            // Count inspection findings created today
            var openNcsToday = await _dbContext.InspectionResults
                .CountAsync(r => r.InspectedAt >= today);

            // For now, we mock these since full operation tracking and ticket systems are in transition
            return Ok(new
            {
                OpenNcsToday = openNcsToday,
                NcRate7d = "2.4%",
                ActiveStreaks = 3,
                OpenTickets = 5
            });
        }
    }
}
