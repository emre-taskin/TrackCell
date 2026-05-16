using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Dtos
{
    public class CreateOperationDto
    {
        [Required]
        public int PartDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OpNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }
}
