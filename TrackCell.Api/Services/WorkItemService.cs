using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using TrackCell.Api.Hubs;
using TrackCell.Api.Models;

using TrackCell.Api.Data;

namespace TrackCell.Api.Services
{
    public class WorkItemService
    {
        private readonly IDatabase _db;
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly OperationHistoryService _historyService;
        private const string RedisKey = "TrackCell:ActiveWorkItems";

        public WorkItemService(IConnectionMultiplexer redis, IHubContext<DashboardHub> hubContext, OperationHistoryService historyService)
        {
            _db = redis.GetDatabase();
            _hubContext = hubContext;
            _historyService = historyService;
        }

        public async Task<WorkItem> StartOperationAsync(WorkItem item)
        {
            item.Id = Guid.NewGuid();
            item.Status = WorkItemStatus.InProcess;
            item.CreatedAt = DateTime.UtcNow;
            
            // 1. Log History to Postgres
            await _historyService.LogActionAsync(
                item.BadgeNumber,
                item.Part,
                item.Serial,
                item.OpNumber,
                "Started"
            );

            // 2. Add to Redis
            var json = JsonSerializer.Serialize(item);
            await _db.HashSetAsync(RedisKey, item.Id.ToString(), json);
            
            // 3. Notify clients
            await _hubContext.Clients.All.SendAsync("UpdateDashboard");
            
            return item;
        }

        public async Task<bool> CompleteOperationAsync(string part, string serial, string opNumber, string badgeNumber, int goodQty = 0, int scrapQty = 0, string? scrapCode = null)
        {
            var activeItems = await GetActiveWorkItemsAsync();
            var item = activeItems.FirstOrDefault(w => 
                w.Part == part && 
                w.Serial == serial && 
                w.OpNumber == opNumber &&
                w.Status == WorkItemStatus.InProcess);

            if (item != null)
            {
                // 1. Log History to Postgres
                await _historyService.LogActionAsync(
                    !string.IsNullOrEmpty(badgeNumber) ? badgeNumber : item.BadgeNumber,
                    item.Part,
                    item.Serial,
                    item.OpNumber,
                    "Completed",
                    goodQty,
                    scrapQty,
                    scrapCode
                );

                // 2. Remove from Redis
                await _db.HashDeleteAsync(RedisKey, item.Id.ToString());
                
                // 3. Notify clients
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
