using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Models;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetByIdAsync([FromQuery] int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound("No record was found for the provided ID.");
            return Ok(user);
        }

        [HttpGet("getUserAccessInfoById")]
        public async Task<IActionResult> GetUserAccessInfoByIdAsync([FromQuery] int id)
        {
            var info = await _dbContext.Users
                .Where(u => u.Id == id)
                .Select(u => new UserAccessInfoDto
                {
                    Id = u.Id,
                    WindowsAccount = u.WindowsAccount,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (info == null) return NotFound("No record was found for the provided ID.");
            return Ok(info);
        }

        [HttpGet("getUserAccessInfoByWindowsAccount")]
        public async Task<IActionResult> GetUserAccessInfoByWindowsAccountAsync([FromQuery] string windowsAccount)
        {
            if (string.IsNullOrWhiteSpace(windowsAccount))
                return BadRequest("windowsAccount is required.");

            var info = await _dbContext.Users
                .Where(u => u.WindowsAccount == windowsAccount)
                .Select(u => new UserAccessInfoDto
                {
                    Id = u.Id,
                    WindowsAccount = u.WindowsAccount,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (info == null) return NotFound("No record was found for the provided windows account.");
            return Ok(info);
        }

        [HttpPost("setRoleToUser")]
        public async Task<IActionResult> SetRoleToUser([FromBody] SetRoleToUserDto setRoleToUserDto)
        {
            if (setRoleToUserDto == null)
                return BadRequest("Request body is required.");

            if (string.IsNullOrWhiteSpace(setRoleToUserDto.Role))
                return BadRequest("Role is required.");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == setRoleToUserDto.UserId);
            if (user == null) return NotFound("No record was found for the provided ID.");

            user.Role = setRoleToUserDto.Role.Trim();
            await _dbContext.SaveChangesAsync();

            return Ok(user);
        }
    }

    public class UserAccessInfoDto
    {
        public int Id { get; set; }
        public string WindowsAccount { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class SetRoleToUserDto
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
