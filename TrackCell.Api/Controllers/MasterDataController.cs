using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;

namespace TrackCell.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MasterDataController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("operators")]
        public async Task<IActionResult> GetOperators()
        {
            var data = await _dbContext.Operators.OrderBy(x => x.Name).ToListAsync();
            return Ok(data);
        }

        [HttpGet("parts")]
        public async Task<IActionResult> GetParts()
        {
            var data = await _dbContext.PartDefinitions.OrderBy(x => x.PartNumber).ToListAsync();
            return Ok(data);
        }

        [HttpGet("operations")]
        public async Task<IActionResult> GetOperations()
        {
            var data = await _dbContext.OperationDefinitions.OrderBy(x => x.OpNumber).ToListAsync();
            return Ok(data);
        }
    }
}
