using System;

namespace Memstate.Models.Relational
{
    public class EntityKey : Entity
    {
        public EntityKey(IEntity entity)
        {
            Id = entity.Id;
            Version = entity.Version;
            Type = entity.GetType();
        }

        public Type Type { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case EntityKey entityKey:
                    return entityKey.Id == Id && entityKey.Type == Type && entityKey.Version == Version;

                default:
                    return false;
            }
        }
    }
}