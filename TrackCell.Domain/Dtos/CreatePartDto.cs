using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Dtos
{
    public class CreatePartDto
    {
        [Required]
        [MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }
}
