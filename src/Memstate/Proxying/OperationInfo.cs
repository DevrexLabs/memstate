using System;
using System.Reflection;

namespace Memstate
{
    internal abstract class OperationInfo<T>
    {
        public readonly MethodInfo MethodInfo;

        public readonly OperationAttribute OperationAttribute;

        protected OperationInfo(MethodInfo methodInfo, OperationAttribute operationAttribute)
        {
            MethodInfo = methodInfo;
            OperationAttribute = operationAttribute;
        }

        public bool IsAllowed => OperationAttribute.Type != OperationType.Disallowed;

        public bool? InputIsIsolated
        {
            get
            {
                if (OperationAttribute.Isolation.HasFlag(IsolationLevel.Input))
                {
                    return true;
                }

                return null;
            }
        }

        public bool? ResultIsIsolated
        {
            get
            {
                if (OperationAttribute.Isolation.HasFlag(IsolationLevel.Output))
                {
                    return true;
                }

                return null;
            }
        }

        protected bool IsMapped => OperationAttribute.MapTo != null;

        public object Execute(Client<T> client, MethodCall methodCall, string signature)
        {
            if (IsMapped && TryGetMappedOperation(methodCall, out var mappedOperation))
            {
                return ExecuteMapped(client, methodCall, mappedOperation);
            }

            return ExecuteProxy(client, methodCall, signature);
        }

        protected abstract object ExecuteMapped(Client<T> client, MethodCall methodCall, object mappedOperation);

        protected abstract object ExecuteProxy(Client<T> engine, MethodCall methodCall, string signature);

        /// <summary>
        /// If operation attribute had a MapTo property selecting a Command or
        /// Query type to map to, return an instance of that type, otherwise null
        /// </summary>
        /// <returns></returns>
        private bool TryGetMappedOperation(MethodCall methodCall, out object mappedOperation)
        {
            try
            {
                mappedOperation = Activator.CreateInstance(OperationAttribute.MapTo, methodCall.Args);
                return true;
            }
            catch (Exception)
            {
                var errorMessage = $"Failed to map method {methodCall.TargetMethod.Name} " +
                                   $"to {OperationAttribute.MapTo.Name}, no matching constructor. " +
                                   "Add a constructor with the same arguments as the method.";

                // TODO: Log the exception.
                mappedOperation = null;

                return false;
            }
        }
    }
}