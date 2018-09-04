using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Memstate
{
    internal class NotAllowedOperation<T> : OperationInfo<T>
    {
        public NotAllowedOperation(MethodInfo methodInfo, OperationAttribute operationAttribute)
            : base(methodInfo, operationAttribute)
        {
        }

        protected override Task<object> ExecuteMapped(Client<T> client, MethodCall methodCall, object mappedOperation)
        {
            throw new NotSupportedException("Proxy method not allowed");
        }

        protected override Task<object> ExecuteProxy(Client<T> engine, MethodCall methodCall, string signature)
        {
            throw new NotSupportedException("Proxy method not allowed");
        }
    }
}