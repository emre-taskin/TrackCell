using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrackCell.API.Utils;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [Authorize(Policy = Policy.Name.AuthorizationRead)]
        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [Authorize(Policy = Policy.Name.AuthorizationRead)]
        [HttpGet("getUserAccessInfoById")]
        public async Task<IActionResult> GetUserAccessInfoById([FromQuery] int id)
        {
            var info = await _service.GetUserAccessInfoByIdAsync(id);
            if (info == null) return NotFound();
            return Ok(info);
        }

        [Authorize(Policy = Policy.Name.AuthorizationRead)]
        [HttpGet("getUserAccessInfoByWindowsAccount")]
        public async Task<IActionResult> GetUserAccessInfoByWindowsAccount([FromQuery] string windowsAccount)
        {
            if (string.IsNullOrWhiteSpace(windowsAccount))
                return BadRequest("windowsAccount is required.");

            var info = await _service.GetUserAccessInfoByWindowsAccountAsync(windowsAccount);
            if (info == null) return NotFound();
            return Ok(info);
        }

        [Authorize(Policy = Policy.Name.AuthorizationWrite)]
        [HttpPost("setRoleToUser")]
        public async Task<IActionResult> SetRoleToUser([FromBody] SetRoleToUserRequest request)
        {
            if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.Role))
                return BadRequest("UserId and Role are required.");

            var info = await _service.SetRoleToUserAsync(request.UserId, request.Role);
            if (info == null) return NotFound();
            return Ok(info);
        }

        [HttpGet("byRole/{role}")]
        public async Task<IActionResult> GetByRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest("role is required.");

            var users = await _service.GetByRoleAsync(role);
            return Ok(users);
        }

        [HttpGet("byBadge/{badgeNumber}")]
        public async Task<IActionResult> GetByBadge(string badgeNumber)
        {
            if (string.IsNullOrWhiteSpace(badgeNumber))
                return BadRequest("badgeNumber is required.");

            var user = await _service.GetByBadgeAsync(badgeNumber);
            if (user == null) return NotFound();
            return Ok(user);
        }
    }
}
