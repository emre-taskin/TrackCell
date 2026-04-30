using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Models;

namespace TrackCell.Api.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly AppDbContext _dbContext;

        public MasterDataService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Operators
        public async Task<IEnumerable<Operator>> GetOperatorsAsync()
        {
            return await _dbContext.Operators.OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<Operator?> GetOperatorByBadgeAsync(string badgeNumber)
        {
            return await _dbContext.Operators
                .FirstOrDefaultAsync(x => x.BadgeNumber == badgeNumber);
        }

        public async Task<Operator> CreateOperatorAsync(Operator op)
        {
            _dbContext.Operators.Add(op);
            await _dbContext.SaveChangesAsync();
            return op;
        }

        public async Task<bool> UpdateOperatorAsync(Operator op)
        {
            var existing = await _dbContext.Operators.FindAsync(op.Id);
            if (existing == null) return false;

            existing.BadgeNumber = op.BadgeNumber;
            existing.Name = op.Name;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOperatorAsync(int id)
        {
            var op = await _dbContext.Operators.FindAsync(id);
            if (op == null) return false;

            _dbContext.Operators.Remove(op);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Parts
        public async Task<IEnumerable<PartDefinition>> GetPartsAsync()
        {
            return await _dbContext.PartDefinitions.OrderBy(x => x.PartNumber).ToListAsync();
        }

        public async Task<PartDefinition?> GetPartByNumberAsync(string partNumber)
        {
            return await _dbContext.PartDefinitions.FirstOrDefaultAsync(p => p.PartNumber == partNumber);
        }

        public async Task<PartDefinition> CreatePartAsync(PartDefinition part)
        {
            _dbContext.PartDefinitions.Add(part);
            await _dbContext.SaveChangesAsync();
            return part;
        }

        public async Task<bool> UpdatePartAsync(PartDefinition part)
        {
            var existing = await _dbContext.PartDefinitions.FindAsync(part.Id);
            if (existing == null) return false;

            existing.PartNumber = part.PartNumber;
            existing.Description = part.Description;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePartAsync(int id)
        {
            var part = await _dbContext.PartDefinitions.FindAsync(id);
            if (part == null) return false;

            _dbContext.PartDefinitions.Remove(part);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Operations
        public async Task<IEnumerable<OperationDefinition>> GetOperationsAsync()
        {
            return await _dbContext.OperationDefinitions.OrderBy(x => x.OpNumber).ToListAsync();
        }

        public async Task<OperationDefinition?> GetOperationByNumberAsync(string opNumber)
        {
            return await _dbContext.OperationDefinitions.FirstOrDefaultAsync(o => o.OpNumber == opNumber);
        }

        public async Task<OperationDefinition> CreateOperationAsync(OperationDefinition op)
        {
            _dbContext.OperationDefinitions.Add(op);
            await _dbContext.SaveChangesAsync();
            return op;
        }

        public async Task<bool> UpdateOperationAsync(OperationDefinition op)
        {
            var existing = await _dbContext.OperationDefinitions.FindAsync(op.Id);
            if (existing == null) return false;

            existing.OpNumber = op.OpNumber;
            existing.Description = op.Description;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOperationAsync(int id)
        {
            var op = await _dbContext.OperationDefinitions.FindAsync(id);
            if (op == null) return false;

            _dbContext.OperationDefinitions.Remove(op);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Serial History
        public async Task<SerialHistoryDto?> GetSerialHistoryAsync(string serialNumber)
        {
            var history = await _dbContext.OperationHistories
                .Where(h => h.SerialNumber == serialNumber)
                .OrderBy(h => h.Timestamp)
                .ToListAsync();

            if (history.Count == 0) return null;

            var partNumber = history.Last().PartNumber;
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
