using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class InspectionResultService : IInspectionResultService
    {
        private readonly ApplicationDbContext _dbContext;

        public InspectionResultService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<InspectionResultDto>> ListAsync(int partImageId)
        {
            var results = await _dbContext.InspectionResults
                .Include(r => r.PartSerial)
                .Where(r => r.PartImageId == partImageId)
                .OrderByDescending(r => r.InspectedAt)
                .ToListAsync();
            return results.Select(ToDto).ToList();
        }

        public async Task<HeatmapResponseDto?> GetHeatmapAsync(int partImageId, int? nonConformanceId)
        {
            var image = await _dbContext.PartImages
                .Include(p => p.Zones)
                .FirstOrDefaultAsync(p => p.Id == partImageId);
            if (image == null) return null;

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

            return new HeatmapResponseDto
            {
                PartImageId = partImageId,
                NonConformanceId = nonConformanceId,
                MaxCount = zones.Count == 0 ? 0 : zones.Max(z => z.Count),
                TotalCount = zones.Sum(z => z.Count),
                Zones = zones
            };
        }

        public async Task<(InspectionResultDto? Result, string? Error)> CreateAsync(CreateInspectionResultRequest body)
        {
            var zone = await _dbContext.ImageZones
                .FirstOrDefaultAsync(z => z.Id == body.ImageZoneId && z.PartImageId == body.PartImageId);
            if (zone == null) return (null, "Zone does not belong to the supplied part image.");

            var ncExists = await _dbContext.NonConformances.AnyAsync(n => n.Id == body.NonConformanceId);
            if (!ncExists) return (null, "Unknown nonConformanceId.");

            var entity = new InspectionResult
            {
                PartImageId = body.PartImageId,
                ImageZoneId = body.ImageZoneId,
                NonConformanceId = body.NonConformanceId,
                PartSerialId = body.PartSerialId,
                Notes = string.IsNullOrWhiteSpace(body.Notes) ? null : body.Notes.Trim(),
                InspectedAt = DateTime.UtcNow
            };
            _dbContext.InspectionResults.Add(entity);
            await _dbContext.SaveChangesAsync();
            return (ToDto(entity), null);
        }

        private static InspectionResultDto ToDto(InspectionResult r) => new()
        {
            Id = r.Id,
            PartImageId = r.PartImageId,
            ImageZoneId = r.ImageZoneId,
            NonConformanceId = r.NonConformanceId,
            PartSerialId = r.PartSerialId,
            SerialNumber = r.PartSerial?.SerialNumber,
            Notes = r.Notes,
            InspectedAt = r.InspectedAt
        };
    }
}
