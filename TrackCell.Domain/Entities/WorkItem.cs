using System;
using TrackCell.Domain.Enums;

namespace TrackCell.Domain.Entities
{
    public class WorkItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string BadgeNumber { get; set; } = string.Empty;
        public string Part { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
        public string OpNumber { get; set; } = string.Empty;
        public WorkItemStatus Status { get; set; } = WorkItemStatus.InProcess;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
