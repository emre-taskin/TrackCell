using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Entities;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NonConformancesController : ControllerBase
    {
        private readonly INonConformanceService _service;

        public NonConformancesController(INonConformanceService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NonConformance body)
        {
            var (item, error, conflict) = await _service.CreateAsync(body);
            if (error != null)
            {
                return conflict ? Conflict(error) : BadRequest(error);
            }
            return CreatedAtAction(nameof(GetAll), new { id = item!.Id }, item);
        }
    }
}
