using System.Collections.Generic;

namespace TrackCell.Domain.Dtos
{
    public class SerialHistoryDto
    {
        public int PartSerialId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string PartDescription { get; set; } = string.Empty;
        public List<string> CompletedOps { get; set; } = new();
        public List<string> InProcessOps { get; set; } = new();
    }
}
