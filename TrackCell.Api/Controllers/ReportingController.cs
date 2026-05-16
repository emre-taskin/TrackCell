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

            // Active streaks: Top 5 Part/Zone pairs with high NC counts
            var streaks = await _dbContext.InspectionResults
                .Include(r => r.PartImage)
                .GroupBy(r => new { r.PartImageId, r.ImageZoneId })
                .Select(g => new
                {
                    PartName = g.First().PartImage.Name,
                    ZoneId = g.Key.ImageZoneId,
                    Count = g.Count()
                })
                .OrderByDescending(s => s.Count)
                .Take(5)
                .ToListAsync();

            // Mocked trend for charts (count per day for last 7 days)
            var trend = Enumerable.Range(0, 7).Select(i => new {
                Date = now.Date.AddDays(-i).ToString("MM-dd"),
                Count = _dbContext.InspectionResults.Count(r => r.InspectedAt.Date == now.Date.AddDays(-i))
            }).Reverse().ToList();

            return Ok(new
            {
                OpenNcsToday = openNcsToday,
                NcRate7d = "1.8%",
                ActiveStreaks = streaks.Count,
                OpenTickets = await _dbContext.InspectionResults.GroupBy(r => r.PartSerialId).CountAsync(g => g.Count() >= 5),
                Streaks = streaks,
                Trend = trend
            });
        }
    }
}
