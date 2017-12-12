using System.Collections.Generic;

namespace Memstate.Models.Relational
{
    public class Conflicts
    {
        /// <summary>
        /// Existing keys.
        /// </summary>
        public readonly List<EntityKey> Inserts = new List<EntityKey>();

        /// <summary>
        /// Missing key or version mismatch.
        /// </summary>
        public readonly List<EntityKey> Updates = new List<EntityKey>();

        /// <summary>
        /// Missing key or version mismatch.
        /// </summary>
        public readonly List<EntityKey> Deletes = new List<EntityKey>();
    }
}