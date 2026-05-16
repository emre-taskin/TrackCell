using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Dtos
{
    public class InspectionResultDto
    {
        public int Id { get; set; }
        public int PartImageId { get; set; }
        public int ImageZoneId { get; set; }
        public int NonConformanceId { get; set; }
        public string? SerialNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime InspectedAt { get; set; }
    }

    public class CreateInspectionResultRequest
    {
        [Required] public int PartImageId { get; set; }
        [Required] public int ImageZoneId { get; set; }
        [Required] public int NonConformanceId { get; set; }
        [MaxLength(100)] public string? SerialNumber { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public class HeatmapZoneDto
    {
        public int ZoneId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        /// <summary>Total inspection results for this zone (filtered by NC if requested).</summary>
        public int Count { get; set; }
        /// <summary>Breakdown of counts per NonConformanceId. Always present even when filtering.</summary>
        public Dictionary<int, int> CountsByNonConformance { get; set; } = new();
    }

    public class HeatmapResponseDto
    {
        public int PartImageId { get; set; }
        public int? NonConformanceId { get; set; }
        public int MaxCount { get; set; }
        public int TotalCount { get; set; }
        public List<HeatmapZoneDto> Zones { get; set; } = new();
    }
}
