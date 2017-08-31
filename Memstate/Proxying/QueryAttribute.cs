using System;
using System.Reflection;

namespace Memstate
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class QueryAttribute : OperationAttribute
    {
        public static readonly OperationAttribute Default = new QueryAttribute();

        public QueryAttribute()
        {
            Type = OperationType.Query;
        }

        internal override OperationInfo<T> ToOperationInfo<T>(MethodInfo methodInfo)
        {
            return new QueryInfo<T>(methodInfo, this);
        }
    }
}