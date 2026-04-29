using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrackCell.Api.Data;
using TrackCell.Api.Models;

namespace TrackCell.Api.Services
{
    public class OperationDefinitionRepository
    {
        private readonly AppDbContext _context;

        public OperationDefinitionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OperationDefinition>> FindAsync(Expression<Func<OperationDefinition, bool>> predicate)
        {
            return await _context.OperationDefinitions.Where(predicate).ToListAsync();
        }

        public IQueryable<OperationDefinition> GetAll()
        {
            return _context.OperationDefinitions.AsQueryable();
        }
    }
}
