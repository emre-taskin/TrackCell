using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class PartSerial : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PartDefinitionId { get; set; }

        public PartDefinition? PartDefinition { get; set; }

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;
    }
}
