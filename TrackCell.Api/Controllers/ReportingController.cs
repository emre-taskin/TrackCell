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
            var now = DateTime.UtcNow;
            var todayStart = now.Date;

            // Count inspection findings created since start of UTC day
            var openNcsToday = await _dbContext.InspectionResults
                .CountAsync(r => r.InspectedAt >= todayStart);

            // Active streaks: Serials with 3 or more findings (threshold for ticket is usually 5)
            var activeStreaks = await _dbContext.InspectionResults
                .GroupBy(r => r.PartSerialId)
                .Where(g => g.Count() >= 3 && g.Count() < 5)
                .CountAsync();

            // Open tickets: Serials with 5 or more findings (auto-ticketed)
            var openTickets = await _dbContext.InspectionResults
                .GroupBy(r => r.PartSerialId)
                .Where(g => g.Count() >= 5)
                .CountAsync();

            // NC Rate: Mocked for now as we don't have total operation volume yet
            // In a real system, this would be findings / total_inspections
            var ncRate = "1.8%";

            return Ok(new
            {
                OpenNcsToday = openNcsToday,
                NcRate7d = ncRate,
                ActiveStreaks = activeStreaks,
                OpenTickets = openTickets
            });
        }
    }
}
