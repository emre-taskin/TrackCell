using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class Operator : IEntity
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
}
