using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using TrackCell.API.Hubs;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Entities;
using TrackCell.Domain.Enums;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class OperationHistoryService : IOperationHistoryService
    {
        private readonly IDatabase _db;
        private readonly IHubContext<DashboardHub> _hubContext;
        private const string RedisKey = "TrackCell:ActiveOperationHistories";

        private readonly ApplicationDbContext _dbContext;

        public OperationHistoryService(IConnectionMultiplexer redis, IHubContext<DashboardHub> hubContext, ApplicationDbContext dbContext)
        {
            _db = redis.GetDatabase();
            _hubContext = hubContext;
            _dbContext = dbContext;
        }

        public async Task<OperationHistory> StartOperationAsync(OperationHistory item)
        {
            var partSerial = await _dbContext.PartSerials
                .Include(p => p.PartDefinition)
                .FirstOrDefaultAsync(p => p.Id == item.PartSerialId);

            if (partSerial != null)
            {
                item.Part = partSerial.PartDefinition?.PartNumber ?? "";
                item.Serial = partSerial.SerialNumber;
            }

            item.Id = Guid.NewGuid();
            item.Status = OperationHistoryStatus.InProcess;
            item.CreatedAt = DateTime.UtcNow;

            var json = JsonSerializer.Serialize(item);
            await _db.HashSetAsync(RedisKey, item.Id.ToString(), json);

            await _hubContext.Clients.All.SendAsync("UpdateDashboard");

            return item;
        }

        public async Task<bool> CompleteOperationAsync(int partSerialId, string opNumber, string badgeNumber)
        {
            var activeItems = await GetActiveOperationHistoriesAsync();
            var item = activeItems.FirstOrDefault(w =>
                w.PartSerialId == partSerialId &&
                w.OpNumber == opNumber &&
                w.Status == OperationHistoryStatus.InProcess);

            if (item != null)
            {
                await _db.HashDeleteAsync(RedisKey, item.Id.ToString());

                await _hubContext.Clients.All.SendAsync("UpdateDashboard");
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<OperationHistory>> GetActiveOperationHistoriesAsync()
        {
            var entries = await _db.HashGetAllAsync(RedisKey);
            var items = new List<OperationHistory>();

            foreach (var entry in entries)
            {
                if (entry.Value.HasValue)
                {
                    var item = JsonSerializer.Deserialize<OperationHistory>((string)entry.Value!);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }

            return items.OrderByDescending(w => w.CreatedAt);
        }
    }
}
