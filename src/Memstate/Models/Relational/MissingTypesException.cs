using System;

namespace Memstate.Models.Relational
{
    public class MissingTypesException : Exception
    {
        public readonly Type[] Types;

        public MissingTypesException(Type[] types)
        {
            Types = types;
        }
    }
}