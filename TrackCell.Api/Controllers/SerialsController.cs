using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("serial")]
    public class SerialsController : ControllerBase
    {
        private readonly ISerialService _service;

        public SerialsController(ISerialService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [HttpGet("byPart/{partId}")]
        public async Task<IActionResult> GetSerialsByPart(int partId)
        {
            var data = await _service.GetByPartAsync(partId);
            return Ok(data);
        }

        [HttpGet("lookup/{serialNumber}")]
        public async Task<IActionResult> LookupSerial(string serialNumber)
        {
            var (result, error) = await _service.LookupAsync(serialNumber);
            if (error != null)
            {
                if (error.Contains("required")) return BadRequest(error);
                return NotFound(error);
            }
            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddSerial([FromBody] CreatePartSerialDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (serial, error) = await _service.AddAsync(dto);
            if (error != null)
            {
                if (error.Contains("not found")) return NotFound(error);
                return BadRequest(error);
            }
            return Ok(serial);
        }
    }
}
