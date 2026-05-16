using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;

namespace TrackCell.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);
        Task<UserAccessInfoDto?> GetUserAccessInfoByIdAsync(int id);
        Task<UserAccessInfoDto?> GetUserAccessInfoByWindowsAccountAsync(string windowsAccount);
        Task<UserAccessInfoDto?> SetRoleToUserAsync(int userId, string role);
        Task<List<UserSummaryDto>> GetByRoleAsync(string role);
        Task<UserSummaryDto?> GetByBadgeAsync(string badgeNumber);
    }
}
