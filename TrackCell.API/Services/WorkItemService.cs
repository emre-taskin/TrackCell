using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using TrackCell.API.Hubs;
using TrackCell.Domain.Entities;
using TrackCell.Domain.Enums;

namespace TrackCell.API.Services
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

            await _historyService.LogActionAsync(
                item.BadgeNumber,
                item.Part,
                item.Serial,
                item.OpNumber,
                "Started"
            );

            var json = JsonSerializer.Serialize(item);
            await _db.HashSetAsync(RedisKey, item.Id.ToString(), json);

            await _hubContext.Clients.All.SendAsync("UpdateDashboard");

            return item;
        }

        public async Task<bool> CompleteOperationAsync(string part, string serial, string opNumber, string badgeNumber)
        {
            var activeItems = await GetActiveWorkItemsAsync();
            var item = activeItems.FirstOrDefault(w =>
                w.Part == part &&
                w.Serial == serial &&
                w.OpNumber == opNumber &&
                w.Status == WorkItemStatus.InProcess);

            if (item != null)
            {
                await _historyService.LogActionAsync(
                    !string.IsNullOrEmpty(badgeNumber) ? badgeNumber : item.BadgeNumber,
                    item.Part,
                    item.Serial,
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
