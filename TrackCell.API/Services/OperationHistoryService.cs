using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class OperationHistoryService
    {
        private readonly ApplicationDbContext _dbContext;

        public OperationHistoryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogActionAsync(string badgeNumber, int partSerialId, string opNumber, string actionLevel)
        {
            _dbContext.OperationHistories.Add(new OperationHistory
            {
                BadgeNumber = badgeNumber,
                PartSerialId = partSerialId,
                OpNumber = opNumber,
                ActionLevel = actionLevel,
                Timestamp = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<SerialHistoryDto?> GetSerialHistoryAsync(string serialNumber)
        {
            var history = await _dbContext.OperationHistories
                .Include(h => h.PartSerial)
                .ThenInclude(p => p.PartDefinition)
                .Where(h => h.PartSerial!.SerialNumber == serialNumber)
                .OrderBy(h => h.Timestamp)
                .ToListAsync();

            if (history.Count == 0) return null;

            var lastRecord = history.Last();
            var partSerial = lastRecord.PartSerial;
            var part = partSerial?.PartDefinition;
            var partNumber = part?.PartNumber ?? "";

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
                PartSerialId = partSerial?.Id ?? 0,
                SerialNumber = serialNumber,
                PartNumber = partNumber,
                PartDescription = part?.Description ?? "",
                CompletedOps = completedOps,
                InProcessOps = startedOps
            };
        }
    }
}
