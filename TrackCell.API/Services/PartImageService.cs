using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class PartImageService : IPartImageService
    {
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/png", "image/jpeg", "image/jpg", "image/webp", "image/gif"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public PartImageService(ApplicationDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        public async Task<List<PartImageDto>> ListAsync(int partDefinitionId)
        {
            var images = await _dbContext.PartImages
                .Where(p => p.PartDefinitionId == partDefinitionId)
                .Include(p => p.Zones)
                    .ThenInclude(z => z.NonConformances)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();

            return images.Select(ToDto).ToList();
        }

        public async Task<PartImageDto?> GetByIdAsync(int id)
        {
            var img = await _dbContext.PartImages
                .Include(p => p.Zones)
                    .ThenInclude(z => z.NonConformances)
                .FirstOrDefaultAsync(p => p.Id == id);
            return img == null ? null : ToDto(img);
        }

        public async Task<(PartImageDto? Image, string? Error)> UploadAsync(PartImageUploadInput input)
        {
            if (input.PartDefinitionId <= 0) return (null, "partDefinitionId is required.");
            if (input.Content == null || input.Length == 0) return (null, "file is required.");
            if (string.IsNullOrWhiteSpace(input.Name)) return (null, "name is required.");
            if (!AllowedContentTypes.Contains(input.ContentType))
                return (null, $"Unsupported content type: {input.ContentType}");

            var partExists = await _dbContext.PartDefinitions.AnyAsync(p => p.Id == input.PartDefinitionId);
            if (!partExists) return (null, $"PartDefinition {input.PartDefinitionId} not found.");

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }
            var folder = Path.Combine(webRoot, "uploads", "parts");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(input.FileName);
            if (string.IsNullOrEmpty(ext))
            {
                ext = input.ContentType switch
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

            await using (var stream = File.Create(fullPath))
            {
                await input.Content.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/parts/{storedName}";
            var entity = new PartImage
            {
                PartDefinitionId = input.PartDefinitionId,
                Name = input.Name.Trim(),
                ImageUrl = relativeUrl,
                ContentType = input.ContentType,
                UploadedAt = DateTime.UtcNow
            };
            _dbContext.PartImages.Add(entity);
            await _dbContext.SaveChangesAsync();

            return (ToDto(entity), null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var img = await _dbContext.PartImages.FirstOrDefaultAsync(p => p.Id == id);
            if (img == null) return false;

            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var relative = img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(webRoot, relative);
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            catch { /* ignore */ }

            _dbContext.PartImages.Remove(img);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<PartImageDto?> SaveZonesAsync(int id, SaveZonesRequest request)
        {
            var img = await _dbContext.PartImages
                .Include(p => p.Zones)
                    .ThenInclude(z => z.NonConformances)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (img == null) return null;

            var validNcIds = await _dbContext.NonConformances
                .Select(n => n.Id)
                .ToListAsync();
            var validSet = new HashSet<int>(validNcIds);

            _dbContext.ImageZones.RemoveRange(img.Zones);
            await _dbContext.SaveChangesAsync();

            foreach (var z in request.Zones ?? new List<ZoneInput>())
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
            return ToDto(refreshed);
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
