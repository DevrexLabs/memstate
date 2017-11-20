using System;

namespace Memstate.Models.Relational
{
    public abstract class Entity : IEntity
    {
        protected Entity()
            : this(Guid.NewGuid())
        {
        }

        protected Entity(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public EntityKey ToKey()
        {
            return new EntityKey(this);
        }
    }
}