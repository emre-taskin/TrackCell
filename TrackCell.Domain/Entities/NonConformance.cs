using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class NonConformance : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
    }
}
