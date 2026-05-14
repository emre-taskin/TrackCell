using System.Collections.Generic;

namespace TrackCell.Domain.Dtos
{
    public class UserAccessInfoDto
    {
        public int Id { get; set; }
        public string WindowsAccount { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? BadgeNumber { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    public class SetRoleToUserRequest
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
