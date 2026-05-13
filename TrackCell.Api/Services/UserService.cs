using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Models;

namespace TrackCell.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<User?> GetByIdAsync(int id)
        {
            return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task<UserAccessInfo?> GetUserAccessInfoByIdAsync(int id)
        {
            return _dbContext.Users
                .Where(u => u.Id == id)
                .Select(u => new UserAccessInfo
                {
                    Id = u.Id,
                    WindowsAccount = u.WindowsAccount,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();
        }

        public Task<UserAccessInfo?> GetUserAccessInfoByWindowsAccountAsync(string windowsAccount)
        {
            return _dbContext.Users
                .Where(u => u.WindowsAccount == windowsAccount)
                .Select(u => new UserAccessInfo
                {
                    Id = u.Id,
                    WindowsAccount = u.WindowsAccount,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();
        }

        public async Task<User?> SetRoleToUserAsync(int userId, string role)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            user.Role = role.Trim();
            await _dbContext.SaveChangesAsync();
            return user;
        }
    }
}
