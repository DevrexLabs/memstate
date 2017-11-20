using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Models.Relational
{
    public class RelationalModel : IRelationalModel
    {
        private readonly Dictionary<Type, EntitySet> _entities = new Dictionary<Type, EntitySet>();

        public IQueryable<T> From<T>() where T : IEntity
        {
            return For(typeof(T)).Values.Cast<T>().AsQueryable();
        }

        public T TryGetById<T>(Guid id) where T : IEntity
        {
            For(typeof(T)).TryGetValue(id, out var entity);

            return (T) entity;
        }

        public bool Exists<T>() where T : IEntity
        {
            return _entities.ContainsKey(typeof(T));
        }

        public bool Create<T>() where T : IEntity
        {
            if (Exists<T>())
            {
                return false;
            }

            _entities.Add(typeof(T), new EntitySet());

            return true;
        }

        public void Delete(params IEntity[] entities)
        {
            var conflicts = new Conflicts();

            if (!CanDelete(entities, conflicts))
            {
                throw new OptimisticConcurrencyException(conflicts);
            }

            DoDelete(entities);
        }

        public void DoExecute(Batch batch)
        {
            var missingTypes = batch.Types().Except(_entities.Keys).ToArray();

            if (missingTypes.Any())
            {
                throw new MissingTypesException(missingTypes);
            }

            var conflicts = new Conflicts();

            if (!CanExecute(batch, conflicts))
            {
                throw new OptimisticConcurrencyException(conflicts);
            }

            DoUpsert(batch.Updates);
            DoUpsert(batch.Inserts);
            DoDelete(batch.Deletes);
        }

        internal void Insert(params IEntity[] entities)
        {
            var conflicts = new Conflicts();

            if (!CanInsert(entities, conflicts))
            {
                throw new OptimisticConcurrencyException(conflicts);
            }

            DoUpsert(entities);
        }

        internal void Update(params IEntity[] entities)
        {
            var conflicts = new Conflicts();

            if (!CanUpdate(entities, conflicts))
            {
                throw new OptimisticConcurrencyException(conflicts);
            }

            DoUpsert(entities);
        }

        private bool CanExecute(Batch batch, Conflicts conflicts)
        {
            return CanDelete(batch.Deletes, conflicts) &&
                   CanInsert(batch.Inserts, conflicts) &&
                   CanUpdate(batch.Updates, conflicts);
        }

        private bool CanInsert(IEnumerable<IEntity> entities, Conflicts conflicts = null)
        {
            conflicts = conflicts ?? new Conflicts();

            var numberOfConflicts = conflicts.Inserts.Count;

            foreach (var entity in entities)
            {
                var set = For(entity);

                if (set.TryGetValue(entity.Id, out var existing))
                {
                    conflicts.Inserts.Add(new EntityKey(existing));
                }
            }

            return numberOfConflicts == conflicts.Inserts.Count;
        }

        private bool CanUpdate(IEnumerable<IEntity> entities, Conflicts conflicts = null)
        {
            conflicts = conflicts ?? new Conflicts();

            var numberOfConflicts = conflicts.Updates.Count;

            foreach (var entity in entities)
            {
                var set = For(entity);

                if (!set.ContainsKey(entity.Id))
                {
                    conflicts.Updates.Add(new EntityKey(entity) {Version = 0});
                }
                else if (set[entity.Id].Version != entity.Version)
                {
                    conflicts.Updates.Add(new EntityKey(set[entity.Id]));
                }
            }

            return numberOfConflicts == conflicts.Updates.Count;
        }

        private bool CanDelete(IEnumerable<IEntity> entities, Conflicts conflicts)
        {
            conflicts = conflicts ?? new Conflicts();

            var numberOfConflicts = conflicts.Deletes.Count;

            foreach (var entity in entities)
            {
                var set = For(entity);

                if (!set.TryGetValue(entity.Id, out var existing))
                {
                    conflicts.Deletes.Add(new EntityKey(entity) {Version = 0});
                }
                else if (existing.Version != entity.Version)
                {
                    conflicts.Deletes.Add(new EntityKey(existing));
                }
            }

            return numberOfConflicts == conflicts.Deletes.Count;
        }

        private void DoUpsert(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                var set = For(entity);

                set[entity.Id] = entity;

                entity.Version++;
            }
        }

        private void DoDelete(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                var set = For(entity);

                set.Remove(entity.Id);
            }
        }

        private EntitySet For(Type type, EntitySet defaultValue = null)
        {
            _entities.TryGetValue(type, out var result);

            result = result ?? defaultValue;

            if (result == null)
            {
                throw new Exception($"No such entity type: {type.FullName}");
            }

            return result;
        }

        private EntitySet For(IEntity entity, EntitySet defaultValue = null)
        {
            var type = entity is EntityKey key ? key.Type : entity.GetType();

            return For(type, defaultValue);
        }

        private class EntitySet : SortedDictionary<Guid, IEntity>
        {
        }
    }
}