using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using TrackCell.API.Hubs;
using TrackCell.Domain.Entities;
using TrackCell.Domain.Enums;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class WorkItemService
    {
        private readonly IDatabase _db;
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly OperationHistoryService _historyService;
        private const string RedisKey = "TrackCell:ActiveWorkItems";

        private readonly ApplicationDbContext _dbContext;

        public WorkItemService(IConnectionMultiplexer redis, IHubContext<DashboardHub> hubContext, OperationHistoryService historyService, ApplicationDbContext dbContext)
        {
            _db = redis.GetDatabase();
            _hubContext = hubContext;
            _historyService = historyService;
            _dbContext = dbContext;
        }

        public async Task<WorkItem> StartOperationAsync(WorkItem item)
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
            item.Status = WorkItemStatus.InProcess;
            item.CreatedAt = DateTime.UtcNow;

            await _historyService.LogActionAsync(
                item.BadgeNumber,
                item.PartSerialId,
                item.OpNumber,
                "Started"
            );

            var json = JsonSerializer.Serialize(item);
            await _db.HashSetAsync(RedisKey, item.Id.ToString(), json);

            await _hubContext.Clients.All.SendAsync("UpdateDashboard");

            return item;
        }

        public async Task<bool> CompleteOperationAsync(int partSerialId, string opNumber, string badgeNumber)
        {
            var activeItems = await GetActiveWorkItemsAsync();
            var item = activeItems.FirstOrDefault(w =>
                w.PartSerialId == partSerialId &&
                w.OpNumber == opNumber &&
                w.Status == WorkItemStatus.InProcess);

            if (item != null)
            {
                await _historyService.LogActionAsync(
                    !string.IsNullOrEmpty(badgeNumber) ? badgeNumber : item.BadgeNumber,
                    item.PartSerialId,
                    item.OpNumber,
                    "Completed"
                );

                await _db.HashDeleteAsync(RedisKey, item.Id.ToString());

                await _hubContext.Clients.All.SendAsync("UpdateDashboard");
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<WorkItem>> GetActiveWorkItemsAsync()
        {
            var entries = await _db.HashGetAllAsync(RedisKey);
            var items = new List<WorkItem>();

            foreach (var entry in entries)
            {
                if (entry.Value.HasValue)
                {
                    var item = JsonSerializer.Deserialize<WorkItem>((string)entry.Value!);
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
