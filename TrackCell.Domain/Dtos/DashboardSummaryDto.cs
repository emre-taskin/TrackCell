namespace TrackCell.Domain.Dtos
{
    public class DashboardSummaryDto
    {
        public int OpenNcsToday { get; set; }
        public string NcRate7d { get; set; } = string.Empty;
        public int ActiveStreaks { get; set; }
        public int OpenTickets { get; set; }
    }
}
