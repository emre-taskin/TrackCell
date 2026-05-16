using TrackCell.Domain.Entities;

namespace TrackCell.Domain.Dtos
{
    public class SerialLookupDto
    {
        public PartSerial PartSerial { get; set; } = null!;
        public PartDefinition PartDefinition { get; set; } = null!;
        public List<OperationDefinition> Operations { get; set; } = new();
    }
}
