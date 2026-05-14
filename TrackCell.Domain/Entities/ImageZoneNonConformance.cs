namespace TrackCell.Domain.Entities
{
    public class ImageZoneNonConformance
    {
        public int ImageZoneId { get; set; }
        public ImageZone? ImageZone { get; set; }

        public int NonConformanceId { get; set; }
        public NonConformance? NonConformance { get; set; }
    }
}
