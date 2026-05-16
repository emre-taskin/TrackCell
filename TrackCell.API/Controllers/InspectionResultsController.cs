using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InspectionResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public InspectionResultsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int partImageId)
        {
            if (partImageId <= 0) return BadRequest("partImageId is required.");
            var results = await _dbContext.InspectionResults
                .Where(r => r.PartImageId == partImageId)
                .OrderByDescending(r => r.InspectedAt)
                .ToListAsync();
            return Ok(results.Select(ToDto));
        }

        [HttpGet("heatmap")]
        public async Task<IActionResult> Heatmap(
            [FromQuery] int partImageId,
            [FromQuery] int? nonConformanceId)
        {
            if (partImageId <= 0) return BadRequest("partImageId is required.");

            var image = await _dbContext.PartImages
                .Include(p => p.Zones)
                .FirstOrDefaultAsync(p => p.Id == partImageId);
            if (image == null) return NotFound();

            var results = await _dbContext.InspectionResults
                .Where(r => r.PartImageId == partImageId)
                .Select(r => new { r.ImageZoneId, r.NonConformanceId })
                .ToListAsync();

            var zones = image.Zones
                .OrderBy(z => z.Id)
                .Select(z =>
                {
                    var zoneResults = results.Where(r => r.ImageZoneId == z.Id).ToList();
                    var breakdown = zoneResults
                        .GroupBy(r => r.NonConformanceId)
                        .ToDictionary(g => g.Key, g => g.Count());
                    var count = nonConformanceId.HasValue
                        ? (breakdown.TryGetValue(nonConformanceId.Value, out var c) ? c : 0)
                        : zoneResults.Count;
                    return new HeatmapZoneDto
                    {
                        ZoneId = z.Id,
                        Name = z.Name,
                        X = z.X,
                        Y = z.Y,
                        Width = z.Width,
                        Height = z.Height,
                        Count = count,
                        CountsByNonConformance = breakdown
                    };
                })
                .ToList();

            var response = new HeatmapResponseDto
            {
                PartImageId = partImageId,
                NonConformanceId = nonConformanceId,
                MaxCount = zones.Count == 0 ? 0 : zones.Max(z => z.Count),
                TotalCount = zones.Sum(z => z.Count),
                Zones = zones
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInspectionResultRequest body)
        {
            if (body == null) return BadRequest();

            var zone = await _dbContext.ImageZones
                .FirstOrDefaultAsync(z => z.Id == body.ImageZoneId && z.PartImageId == body.PartImageId);
            if (zone == null) return BadRequest("Zone does not belong to the supplied part image.");

            var ncExists = await _dbContext.NonConformances.AnyAsync(n => n.Id == body.NonConformanceId);
            if (!ncExists) return BadRequest("Unknown nonConformanceId.");

            var entity = new InspectionResult
            {
                PartImageId = body.PartImageId,
                ImageZoneId = body.ImageZoneId,
                NonConformanceId = body.NonConformanceId,
                SerialNumber = string.IsNullOrWhiteSpace(body.SerialNumber) ? null : body.SerialNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(body.Notes) ? null : body.Notes.Trim(),
                InspectedAt = DateTime.UtcNow
            };
            _dbContext.InspectionResults.Add(entity);
            await _dbContext.SaveChangesAsync();
            return Ok(ToDto(entity));
        }

        private static InspectionResultDto ToDto(InspectionResult r) => new()
        {
            Id = r.Id,
            PartImageId = r.PartImageId,
            ImageZoneId = r.ImageZoneId,
            NonConformanceId = r.NonConformanceId,
            SerialNumber = r.SerialNumber,
            Notes = r.Notes,
            InspectedAt = r.InspectedAt
        };
    }
}
