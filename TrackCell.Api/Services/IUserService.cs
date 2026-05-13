using System.Threading.Tasks;
using TrackCell.Api.Models;

namespace TrackCell.Api.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);
        Task<UserAccessInfoDto?> GetUserAccessInfoByIdAsync(int id);
        Task<UserAccessInfoDto?> GetUserAccessInfoByWindowsAccountAsync(string windowsAccount);
        Task<UserAccessInfoDto?> SetRoleToUserAsync(int userId, string role);
    }
}
