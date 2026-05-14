using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PartImagesController : ControllerBase
    {
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/png", "image/jpeg", "image/jpg", "image/webp", "image/gif"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public PartImagesController(ApplicationDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int partDefinitionId)
        {
            if (partDefinitionId <= 0) return BadRequest("partDefinitionId is required.");

            var images = await _dbContext.PartImages
                .Where(p => p.PartDefinitionId == partDefinitionId)
                .Include(p => p.Zones)
                    .ThenInclude(z => z.NonConformances)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();

            return Ok(images.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var img = await _dbContext.PartImages
                .Include(p => p.Zones)
                    .ThenInclude(z => z.NonConformances)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (img == null) return NotFound();
            return Ok(ToDto(img));
        }

        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Upload([FromForm] int partDefinitionId, [FromForm] string name, [FromForm] IFormFile file)
        {
            if (partDefinitionId <= 0) return BadRequest("partDefinitionId is required.");
            if (file == null || file.Length == 0) return BadRequest("file is required.");
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("name is required.");
            if (!AllowedContentTypes.Contains(file.ContentType))
                return BadRequest($"Unsupported content type: {file.ContentType}");

            var partExists = await _dbContext.PartDefinitions.AnyAsync(p => p.Id == partDefinitionId);
            if (!partExists) return NotFound($"PartDefinition {partDefinitionId} not found.");

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }
            var folder = Path.Combine(webRoot, "uploads", "parts");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext))
            {
                ext = file.ContentType switch
                {
                    "image/png" => ".png",
                    "image/jpeg" or "image/jpg" => ".jpg",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
                    _ => ".bin"
                };
            }
            var storedName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, storedName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/parts/{storedName}";
            var entity = new PartImage
            {
                PartDefinitionId = partDefinitionId,
                Name = name.Trim(),
                ImageUrl = relativeUrl,
                ContentType = file.ContentType,
                UploadedAt = DateTime.UtcNow
            };
            _dbContext.PartImages.Add(entity);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = entity.Id }, ToDto(entity));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var img = await _dbContext.PartImages.FirstOrDefaultAsync(p => p.Id == id);
            if (img == null) return NotFound();

            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var relative = img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(webRoot, relative);
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }
            catch { /* ignore */ }

            _dbContext.PartImages.Remove(img);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}/zones")]
        public async Task<IActionResult> SaveZones(int id, [FromBody] SaveZonesRequest body)
        {
            var img = await _dbContext.PartImages
                .Include(p => p.Zones)
                    .ThenInclude(z => z.NonConformances)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (img == null) return NotFound();

            var validNcIds = await _dbContext.NonConformances
                .Select(n => n.Id)
                .ToListAsync();
            var validSet = new HashSet<int>(validNcIds);

            _dbContext.ImageZones.RemoveRange(img.Zones);
            await _dbContext.SaveChangesAsync();

            foreach (var z in body.Zones ?? new List<ZoneInput>())
            {
                var zone = new ImageZone
                {
                    PartImageId = img.Id,
                    Name = (z.Name ?? string.Empty).Trim(),
                    X = Clamp01(z.X),
                    Y = Clamp01(z.Y),
                    Width = Clamp01(z.Width),
                    Height = Clamp01(z.Height)
                };
                foreach (var ncId in (z.NonConformanceIds ?? new()).Distinct())
                {
                    if (!validSet.Contains(ncId)) continue;
                    zone.NonConformances.Add(new ImageZoneNonConformance { NonConformanceId = ncId });
                }
                _dbContext.ImageZones.Add(zone);
            }
            await _dbContext.SaveChangesAsync();

            var refreshed = await _dbContext.PartImages
                .Include(p => p.Zones)
                    .ThenInclude(z => z.NonConformances)
                .FirstAsync(p => p.Id == id);
            return Ok(ToDto(refreshed));
        }

        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        private static PartImageDto ToDto(PartImage p) => new()
        {
            Id = p.Id,
            PartDefinitionId = p.PartDefinitionId,
            Name = p.Name,
            ImageUrl = p.ImageUrl,
            UploadedAt = p.UploadedAt,
            Zones = p.Zones.Select(z => new ImageZoneDto
            {
                Id = z.Id,
                Name = z.Name,
                X = z.X,
                Y = z.Y,
                Width = z.Width,
                Height = z.Height,
                NonConformanceIds = z.NonConformances.Select(n => n.NonConformanceId).ToList()
            }).ToList()
        };
    }
}
