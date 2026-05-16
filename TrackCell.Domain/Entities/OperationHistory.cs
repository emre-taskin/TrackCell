using System;
using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class OperationHistory : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BadgeNumber { get; set; } = string.Empty;

        [Required]
        public int PartSerialId { get; set; }

        public PartSerial? PartSerial { get; set; }

        [Required]
        [MaxLength(50)]
        public string OpNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ActionLevel { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
