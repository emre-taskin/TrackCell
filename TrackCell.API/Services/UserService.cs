using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;

        public UserService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<User?> GetByIdAsync(int id)
        {
            return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserAccessInfoDto?> GetUserAccessInfoByIdAsync(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user == null ? null : ToAccessInfo(user);
        }

        public async Task<UserAccessInfoDto?> GetUserAccessInfoByWindowsAccountAsync(string windowsAccount)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.WindowsAccount == windowsAccount);
            return user == null ? null : ToAccessInfo(user);
        }

        public async Task<UserAccessInfoDto?> SetRoleToUserAsync(int userId, string role)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            user.Role = role.Trim();
            await _dbContext.SaveChangesAsync();
            return ToAccessInfo(user);
        }

        public async Task<List<UserSummaryDto>> GetByRoleAsync(string role)
        {
            var normalized = role.Trim();
            var users = await _dbContext.Users
                .Where(u => u.Role == normalized)
                .OrderBy(u => u.DisplayName)
                .ToListAsync();
            return users.Select(ToSummary).ToList();
        }

        public async Task<UserSummaryDto?> GetByBadgeAsync(string badgeNumber)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.BadgeNumber == badgeNumber);
            return user == null ? null : ToSummary(user);
        }

        private static UserSummaryDto ToSummary(User u) => new()
        {
            Id = u.Id,
            DisplayName = u.DisplayName,
            Role = u.Role,
            BadgeNumber = u.BadgeNumber
        };

        private static UserAccessInfoDto ToAccessInfo(User u) => new()
        {
            Id = u.Id,
            WindowsAccount = u.WindowsAccount,
            DisplayName = u.DisplayName,
            Role = u.Role,
            BadgeNumber = u.BadgeNumber,
            Permissions = PermissionsForRole(u.Role)
        };

        private static List<string> PermissionsForRole(string role) => role switch
        {
            "Admin" => new() { "user.read", "user.write", "masterdata.read", "masterdata.write", "operations.read", "operations.write" },
            "Supervisor" => new() { "user.read", "masterdata.read", "operations.read", "operations.write" },
            "Operator" => new() { "masterdata.read", "operations.read", "operations.write" },
            _ => new() { "masterdata.read" }
        };
    }
}
