using System.Reflection;

namespace Memstate
{
    public class DispatchProxy<TModel> : DispatchProxy
    {
        private Client<TModel> _handler;
        private MethodMap<TModel> _methods;

        public void SetClient(Client<TModel> client)
        {
            _handler = client;
            _methods = MethodMap.MapFor<TModel>();
        }

        private static string GetSignature(MethodInfo callMessage)
        {
            return callMessage.IsGenericMethod
                ? callMessage.GetGenericMethodDefinition().ToString()
                : callMessage.ToString();
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var signature = GetSignature(targetMethod);
            var operationInfo = _methods.GetOperationInfo(signature);
            var methodCall = new MethodCall(targetMethod, args);
            var result = operationInfo.Execute(_handler, methodCall, signature).Result;
            return result;
        }
    }
}