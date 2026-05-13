using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Models;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("getUserAccessInfoById")]
        public async Task<IActionResult> GetUserAccessInfoById([FromQuery] int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return Ok(ToAccessInfo(user));
        }

        [HttpGet("getUserAccessInfoByWindowsAccount")]
        public async Task<IActionResult> GetUserAccessInfoByWindowsAccount([FromQuery] string windowsAccount)
        {
            if (string.IsNullOrWhiteSpace(windowsAccount))
                return BadRequest("windowsAccount is required.");

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.WindowsAccount == windowsAccount);
            if (user == null) return NotFound();
            return Ok(ToAccessInfo(user));
        }

        [HttpPost("setRoleToUser")]
        public async Task<IActionResult> SetRoleToUser([FromBody] SetRoleToUserRequest request)
        {
            if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.Role))
                return BadRequest("UserId and Role are required.");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null) return NotFound();

            user.Role = request.Role.Trim();
            await _dbContext.SaveChangesAsync();
            return Ok(ToAccessInfo(user));
        }

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
