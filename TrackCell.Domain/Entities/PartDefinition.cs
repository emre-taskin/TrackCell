using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class PartDefinition : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }
}
