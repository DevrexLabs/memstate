using System;
using System.Reflection;

namespace Memstate
{
    internal class NotAllowedOperation<T> : OperationInfo<T>
    {
        public NotAllowedOperation(MethodInfo methodInfo, OperationAttribute operationAttribute)
            : base(methodInfo, operationAttribute)
        {
        }


        protected override object Execute(Client<T> engine, string signature, object operation, MethodCall methodCall)
        {
            throw new NotSupportedException("Proxy method not allowed");
        }
    }
}