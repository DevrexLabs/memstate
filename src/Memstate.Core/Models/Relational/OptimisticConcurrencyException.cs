using System;

namespace Memstate.Models.Relational
{
    public class OptimisticConcurrencyException : Exception
    {
        public readonly Conflicts Conflicts;

        public OptimisticConcurrencyException(Conflicts conflicts)
        {
            Conflicts = conflicts;
        }
    }
}