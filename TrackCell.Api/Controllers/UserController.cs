using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Api.Constants;
using TrackCell.Api.Services;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [Authorize(Policy = Policy.Name.AuthorizationRead)]
        [HttpGet("getById")]
        public async Task<IActionResult> GetByIdAsync([FromQuery] int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null) return NotFound("No record was found for the provided ID.");
            return Ok(user);
        }

        [Authorize(Policy = Policy.Name.AuthorizationRead)]
        [HttpGet("getUserAccessInfoById")]
        public async Task<IActionResult> GetUserAccessInfoByIdAsync([FromQuery] int id)
        {
            var info = await _service.GetUserAccessInfoByIdAsync(id);
            if (info == null) return NotFound("No record was found for the provided ID.");
            return Ok(info);
        }

        [Authorize(Policy = Policy.Name.AuthorizationRead)]
        [HttpGet("getUserAccessInfoByWindowsAccount")]
        public async Task<IActionResult> GetUserAccessInfoByWindowsAccountAsync([FromQuery] string windowsAccount)
        {
            if (string.IsNullOrWhiteSpace(windowsAccount))
                return BadRequest("windowsAccount is required.");

            var info = await _service.GetUserAccessInfoByWindowsAccountAsync(windowsAccount);
            if (info == null) return NotFound("No record was found for the provided windows account.");
            return Ok(info);
        }

        [Authorize(Policy = Policy.Name.AuthorizationWrite)]
        [HttpPost("setRoleToUser")]
        public async Task<IActionResult> SetRoleToUser([FromBody] SetRoleToUserDto setRoleToUserDto)
        {
            if (setRoleToUserDto == null)
                return BadRequest("Request body is required.");

            if (string.IsNullOrWhiteSpace(setRoleToUserDto.Role))
                return BadRequest("Role is required.");

            var user = await _service.SetRoleToUserAsync(setRoleToUserDto.UserId, setRoleToUserDto.Role);
            if (user == null) return NotFound("No record was found for the provided ID.");

            return Ok(user);
        }
    }

    public class SetRoleToUserDto
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
