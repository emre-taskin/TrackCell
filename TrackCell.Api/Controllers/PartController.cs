using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PartController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public PartController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _dbContext.PartDefinitions.OrderBy(x => x.PartNumber).ToListAsync();
            return Ok(data);
        }
    }
}
