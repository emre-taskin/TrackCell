using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class OperationDefinition : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string OpNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int PartDefinitionId { get; set; }

        public PartDefinition? PartDefinition { get; set; }
    }
}
