using System;
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
            var today = DateTime.UtcNow.Date;

            var openNcsToday = await _dbContext.InspectionResults
                .CountAsync(r => r.InspectedAt >= today);

            return new DashboardSummaryDto
            {
                OpenNcsToday = openNcsToday,
                NcRate7d = "2.4%",
                ActiveStreaks = 3,
                OpenTickets = 5
            };
        }
    }
}
