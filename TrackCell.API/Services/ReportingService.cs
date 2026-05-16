using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;

            var openNcsToday = await _dbContext.InspectionResults
                .CountAsync(r => r.InspectedAt >= todayStart);

            var streaks = await _dbContext.InspectionResults
                .Include(r => r.PartImage)
                .GroupBy(r => new { r.PartImageId, r.ImageZoneId })
                .Select(g => new DashboardStreakDto
                {
                    PartName = g.First().PartImage!.Name,
                    ZoneId = g.Key.ImageZoneId,
                    Count = g.Count()
                })
                .OrderByDescending(s => s.Count)
                .Take(5)
                .ToListAsync();

            var trend = Enumerable.Range(0, 7).Select(i => new DashboardTrendPointDto
            {
                Date = now.Date.AddDays(-i).ToString("MM-dd"),
                Count = _dbContext.InspectionResults.Count(r => r.InspectedAt.Date == now.Date.AddDays(-i))
            }).Reverse().ToList();

            var openTickets = await _dbContext.InspectionResults
                .GroupBy(r => r.PartSerialId)
                .CountAsync(g => g.Count() >= 5);

            return new DashboardSummaryDto
            {
                OpenNcsToday = openNcsToday,
                NcRate7d = "1.8%",
                ActiveStreaks = streaks.Count,
                OpenTickets = openTickets,
                Streaks = streaks,
                Trend = trend
            };
        }
    }
}
