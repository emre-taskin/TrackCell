using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class ServerMetricService
    {
        private readonly ApplicationDbContext _dbContext;

        public ServerMetricService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServerMetric> RecordAsync(ServerMetricDto dto)
        {
            var metric = new ServerMetric
            {
                MachineName = dto.MachineName,
                Timestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp.ToUniversalTime(),
                CpuUsagePercent = dto.CpuUsagePercent,
                TotalMemoryBytes = dto.TotalMemoryBytes,
                AvailableMemoryBytes = dto.AvailableMemoryBytes,
                MemoryUsagePercent = dto.MemoryUsagePercent,
                TotalDiskBytes = dto.TotalDiskBytes,
                AvailableDiskBytes = dto.AvailableDiskBytes,
                DiskUsagePercent = dto.DiskUsagePercent,
                UptimeSeconds = dto.UptimeSeconds,
                HealthStatus = string.IsNullOrWhiteSpace(dto.HealthStatus) ? "Healthy" : dto.HealthStatus
            };

            _dbContext.ServerMetrics.Add(metric);
            await _dbContext.SaveChangesAsync();
            return metric;
        }

        public async Task<List<ServerMetric>> GetRecentAsync(string? machineName, int take)
        {
            var query = _dbContext.ServerMetrics.AsQueryable();
            if (!string.IsNullOrWhiteSpace(machineName))
            {
                query = query.Where(m => m.MachineName == machineName);
            }

            return await query
                .OrderByDescending(m => m.Timestamp)
                .Take(Math.Clamp(take, 1, 1000))
                .ToListAsync();
        }

        public async Task<ServerMetric?> GetLatestAsync(string machineName)
        {
            return await _dbContext.ServerMetrics
                .Where(m => m.MachineName == machineName)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();
        }
    }
}
