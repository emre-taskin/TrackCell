using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackCell.Api.Models
{
    public class Operator
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string BadgeNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class PartDefinition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }

    public class OperationDefinition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string OpNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }

    public class OperationHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BadgeNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string OpNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ActionLevel { get; set; } = string.Empty; // "Started" or "Completed"

        public int GoodQty { get; set; }

        public int ScrapQty { get; set; }

        [MaxLength(50)]
        public string? ScrapCode { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
