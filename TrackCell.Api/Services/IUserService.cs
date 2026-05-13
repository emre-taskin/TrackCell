using System.Threading.Tasks;
using TrackCell.Api.Models;

namespace TrackCell.Api.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);
        Task<UserAccessInfo?> GetUserAccessInfoByIdAsync(int id);
        Task<UserAccessInfo?> GetUserAccessInfoByWindowsAccountAsync(string windowsAccount);
        Task<User?> SetRoleToUserAsync(int userId, string role);
    }

    public class UserAccessInfo
    {
        public int Id { get; set; }
        public string WindowsAccount { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
