using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PartImagesController : ControllerBase
    {
        private readonly IPartImageService _service;

        public PartImagesController(IPartImageService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int partDefinitionId)
        {
            if (partDefinitionId <= 0) return BadRequest("partDefinitionId is required.");
            var images = await _service.ListAsync(partDefinitionId);
            return Ok(images);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var img = await _service.GetByIdAsync(id);
            if (img == null) return NotFound();
            return Ok(img);
        }

        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Upload([FromForm] int partDefinitionId, [FromForm] string name, [FromForm] IFormFile file)
        {
            if (file == null) return BadRequest("file is required.");

            await using var stream = file.OpenReadStream();
            var input = new PartImageUploadInput
            {
                PartDefinitionId = partDefinitionId,
                Name = name ?? string.Empty,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
                Content = stream
            };

            var (image, error) = await _service.UploadAsync(input);
            if (error != null)
            {
                if (error.Contains("not found")) return NotFound(error);
                return BadRequest(error);
            }
            return CreatedAtAction(nameof(GetOne), new { id = image!.Id }, image);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPut("{id:int}/zones")]
        public async Task<IActionResult> SaveZones(int id, [FromBody] SaveZonesRequest body)
        {
            var result = await _service.SaveZonesAsync(id, body);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
