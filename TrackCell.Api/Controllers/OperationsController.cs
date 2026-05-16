using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("operation")]
    public class OperationsController : ControllerBase
    {
        private readonly IOperationService _service;

        public OperationsController(IOperationService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [HttpGet("byPart/{partId}")]
        public async Task<IActionResult> GetOperationsByPart(int partId)
        {
            var data = await _service.GetByPartAsync(partId);
            return Ok(data);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddOperation([FromBody] CreateOperationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (op, error) = await _service.AddAsync(dto);
            if (error != null)
            {
                if (error.Contains("not found")) return NotFound(error);
                return BadRequest(error);
            }
            return Ok(op);
        }
    }
}
