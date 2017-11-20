using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Models.Relational
{
    public class Batch
    {
        internal readonly List<IEntity> Inserts = new List<IEntity>();
        internal readonly List<IEntity> Updates = new List<IEntity>();
        internal readonly List<IEntity> Deletes = new List<IEntity>();

        private readonly ISet<Guid> _uniqueIds = new HashSet<Guid>();

        public void Insert(IEntity entity)
        {
            EnsureUnique(entity);

            Inserts.Add(entity);
        }

        public void Update(IEntity entity)
        {
            EnsureUnique(entity);

            Updates.Add(entity);
        }

        public void Delete(IEntity entity)
        {
            EnsureUnique(entity);

            if (entity.GetType() != typeof(EntityKey))
            {
                entity = new EntityKey(entity);
            }

            Deletes.Add(entity);
        }

        internal IEnumerable<Type> Types()
        {
            return Deletes.Cast<EntityKey>()
                .Select(x => x.Type)
                .Concat(Inserts.Concat(Updates).Select(x => x.GetType()))
                .Distinct();
        }

        private void EnsureUnique(IEntity entity)
        {
            var id = entity.Id;

            if (_uniqueIds.Contains(id))
            {
                throw new ArgumentException($"Duplicate id: {id}", nameof(id));
            }

            _uniqueIds.Add(id);
        }
    }
}