using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Application.Interfaces;
using TrackCell.Domain.Entities;
using TrackCell.Infrastructure.Persistence;

namespace TrackCell.API.Services
{
    public class NonConformanceService : INonConformanceService
    {
        private readonly ApplicationDbContext _dbContext;

        public NonConformanceService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<NonConformance>> GetAllAsync()
        {
            return _dbContext.NonConformances
                .OrderBy(n => n.Code)
                .ToListAsync();
        }

        public async Task<(NonConformance? Item, string? Error, bool Conflict)> CreateAsync(NonConformance body)
        {
            if (string.IsNullOrWhiteSpace(body.Code) || string.IsNullOrWhiteSpace(body.Description))
                return (null, "Code and Description are required.", false);

            var exists = await _dbContext.NonConformances.AnyAsync(n => n.Code == body.Code);
            if (exists) return (null, $"NC with code '{body.Code}' already exists.", true);

            var nc = new NonConformance { Code = body.Code.Trim(), Description = body.Description.Trim() };
            _dbContext.NonConformances.Add(nc);
            await _dbContext.SaveChangesAsync();
            return (nc, null, false);
        }
    }
}
