using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Models;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NonConformancesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public NonConformancesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _dbContext.NonConformances
                .OrderBy(n => n.Code)
                .ToListAsync();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NonConformance body)
        {
            if (string.IsNullOrWhiteSpace(body.Code) || string.IsNullOrWhiteSpace(body.Description))
                return BadRequest("Code and Description are required.");

            var exists = await _dbContext.NonConformances.AnyAsync(n => n.Code == body.Code);
            if (exists) return Conflict($"NC with code '{body.Code}' already exists.");

            var nc = new NonConformance { Code = body.Code.Trim(), Description = body.Description.Trim() };
            _dbContext.NonConformances.Add(nc);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = nc.Id }, nc);
        }
    }
}
