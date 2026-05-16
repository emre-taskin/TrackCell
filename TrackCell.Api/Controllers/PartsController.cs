using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("part")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _service;

        public PartsController(IPartService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<IActionResult> GetParts()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpPost("addPart")]
        public async Task<IActionResult> AddPart([FromBody] CreatePartDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (part, error) = await _service.AddAsync(dto);
            if (error != null) return BadRequest(error);
            return Ok(part);
        }
    }
}
