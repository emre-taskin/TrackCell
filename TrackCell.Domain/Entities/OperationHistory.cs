using System;
using TrackCell.Domain.Enums;

namespace TrackCell.Domain.Entities
{
    public class OperationHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string BadgeNumber { get; set; } = string.Empty;
        public int PartSerialId { get; set; }
        public string Part { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
        public string OpNumber { get; set; } = string.Empty;
        public OperationHistoryStatus Status { get; set; } = OperationHistoryStatus.InProcess;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
