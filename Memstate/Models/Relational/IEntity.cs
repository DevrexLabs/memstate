using System;

namespace Memstate.Models.Relational
{
    public interface IEntity
    {
        Guid Id { get; set; }

        int Version { get; set; }
    }
}