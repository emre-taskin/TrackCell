using System;
using System.Collections.Generic;

namespace TrackCell.Domain.Exceptions
{
    public class EntityNotFoundException<TEntity> : Exception
    {
        public EntityNotFoundException(int id)
            : base($"{typeof(TEntity).Name} with id '{id}' was not found.")
        {
        }
    }

    public class EntitiesNotFoundException<TEntity> : Exception
    {
        public EntitiesNotFoundException(IEnumerable<int> ids)
            : base($"One or more {typeof(TEntity).Name} entities were not found: {string.Join(", ", ids)}")
        {
        }
    }
}
