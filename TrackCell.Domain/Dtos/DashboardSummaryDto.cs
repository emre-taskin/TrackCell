using System.Collections.Generic;

namespace TrackCell.Domain.Dtos
{
    public class DashboardSummaryDto
    {
        public int OpenNcsToday { get; set; }
        public string NcRate7d { get; set; } = string.Empty;
        public int ActiveStreaks { get; set; }
        public int OpenTickets { get; set; }
        public List<DashboardStreakDto> Streaks { get; set; } = new();
        public List<DashboardTrendPointDto> Trend { get; set; } = new();
    }

    public class DashboardStreakDto
    {
        public string PartName { get; set; } = string.Empty;
        public int ZoneId { get; set; }
        public int Count { get; set; }
    }

    public class DashboardTrendPointDto
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
