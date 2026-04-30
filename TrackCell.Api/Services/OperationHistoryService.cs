using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Models;

namespace TrackCell.Api.Services
{
    public class OperationHistoryService
    {
        private readonly AppDbContext _dbContext;

        public OperationHistoryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogActionAsync(string badgeNumber, string partNumber, string serialNumber, string opNumber, string actionLevel)
        {
            _dbContext.OperationHistories.Add(new OperationHistory
            {
                BadgeNumber = badgeNumber,
                PartNumber = partNumber,
                SerialNumber = serialNumber,
                OpNumber = opNumber,
                ActionLevel = actionLevel,
                Timestamp = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<SerialHistoryDto?> GetSerialHistoryAsync(string serialNumber)
        {
            var history = await _dbContext.OperationHistories
                .Where(h => h.SerialNumber == serialNumber)
                .OrderBy(h => h.Timestamp)
                .ToListAsync();

            if (history.Count == 0) return null;

            var lastRecord = history.Last();
            var partNumber = lastRecord.PartNumber;

            var part = await _dbContext.PartDefinitions
                .FirstOrDefaultAsync(p => p.PartNumber == partNumber);

            var completedOps = history
                .Where(h => h.ActionLevel == "Completed")
                .Select(h => h.OpNumber)
                .Distinct()
                .ToList();

            var startedOps = history
                .Where(h => h.ActionLevel == "Started")
                .Select(h => h.OpNumber)
                .Distinct()
                .Where(op => !completedOps.Contains(op))
                .ToList();

            return new SerialHistoryDto
            {
                SerialNumber = serialNumber,
                PartNumber = partNumber,
                PartDescription = part?.Description ?? "",
                CompletedOps = completedOps,
                InProcessOps = startedOps
            };
        }
    }
}
