using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using TrackCell.Domain.Entities;

namespace TrackCell.Application.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : class, IEntity
    {
        Task<TEntity> AddAsync(TEntity entity);

        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);

        IEnumerable<TEntity> Filter(
            Expression<Func<TEntity, bool>> predicate,
            int? page = null,
            int? itemCount = null);

        Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate);

        Task<IEnumerable<TEntity>> GetAllAsync();

        ValueTask<TEntity?> GetByIdAsync(
            int id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);

        Task RemoveAsync(int entityId);

        void RemoveRange(IEnumerable<int> entityIds);
    }
}
