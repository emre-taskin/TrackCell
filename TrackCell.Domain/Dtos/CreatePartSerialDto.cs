using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Dtos
{
    public class CreatePartSerialDto
    {
        [Required]
        public int PartDefinitionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;
    }
}
