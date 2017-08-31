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
                if (OperationAttribute.Isolation.HasFlag(IsolationLevel.Input)) return true;
                return null;
            }
        }

        public bool? ResultIsIsolated
        {
            get
            {
                if (OperationAttribute.Isolation.HasFlag(IsolationLevel.Output)) return true;
                return null;
            }
        }

        protected bool IsMapped => OperationAttribute.MapTo != null;

        /// <summary>
        /// If operation attribute had a MapTo property selecting a Command or
        /// Query type to map to, return an instance of that type, otherwise null
        /// </summary>
        /// <returns></returns>
        private object GetMappedOperation(MethodCall methodCall)
        {
            var mapTo = OperationAttribute.MapTo;
            /*
            var constructor = mapTo.GetConstructor(callMessage.InArgs.Select(args => args.GetType()).ToArray());
            if (constructor == null) return null;
            return constructor.Invoke(callMessage.InArgs);
            */
            return null;
        }

        public object Execute(Client<T> engine, MethodCall callMessage, string signature)
        {
            var operation = IsMapped ? GetMappedOperation(callMessage) : null;
            return Execute(engine, signature, operation, callMessage);
        }

        protected abstract object Execute(Client<T> engine, string signature, object operation, MethodCall methodCall);
    }
}