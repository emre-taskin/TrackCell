using System.ComponentModel.DataAnnotations;

namespace TrackCell.Domain.Entities
{
    public class User : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string WindowsAccount { get; set; } = string.Empty;

        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Operator";

        [MaxLength(50)]
        public string? BadgeNumber { get; set; }
    }
}
