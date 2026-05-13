using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TrackCell.Api.Exceptions;
using TrackCell.Api.Models;

namespace TrackCell.Api.Data.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, IEntity
    {
        protected readonly AppDbContext Context;

        public BaseRepository(AppDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            var entry = await Context.Set<TEntity>().AddAsync(entity);
            return entry.Entity;
        }

        public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await Context.Set<TEntity>().AddRangeAsync(entities);
            return entities;
        }

        public IEnumerable<TEntity> Filter(
            Expression<Func<TEntity, bool>> predicate,
            int? page = null,
            int? itemCount = null)
        {
            IQueryable<TEntity> entities = Context.Set<TEntity>().Where(predicate);
            if (page is not null && itemCount is not null)
            {
                entities = entities.Skip((page.Value - 1) * itemCount.Value).Take(itemCount.Value);
            }
            return entities;
        }

        public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Set<TEntity>().SingleOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await Context.Set<TEntity>().ToListAsync();
        }

        public async ValueTask<TEntity?> GetByIdAsync(
            int id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();
            if (include is not null)
            {
                query = include(query);
            }
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task RemoveAsync(int entityId)
        {
            var foundEntity = await FindAsync(e => e.Id == entityId)
                ?? throw new EntityNotFoundException<TEntity>(entityId);
            Context.Set<TEntity>().Remove(foundEntity);
        }

        public void RemoveRange(IEnumerable<int> entityIds)
        {
            var ids = entityIds.ToList();
            var foundEntities = Filter(e => ids.Contains(e.Id)).ToList();
            if (foundEntities.Count != ids.Count)
            {
                var missingIds = ids.Where(id => foundEntities.All(e => e.Id != id));
                throw new EntitiesNotFoundException<TEntity>(missingIds);
            }
            Context.Set<TEntity>().RemoveRange(foundEntities);
        }
    }
}
