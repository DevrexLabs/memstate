using System.Reflection;

namespace Memstate
{
    internal sealed class QueryInfo<T> : OperationInfo<T>
    {
        public QueryInfo(MethodInfo methodInfo, OperationAttribute attribute)
            : base(methodInfo, attribute)
        {
        }

        protected override object ExecuteMapped(Client<T> client, MethodCall methodCall, object mappedQuery)
        {
            return client.Execute((Query) mappedQuery);
        }

        protected override object ExecuteProxy(Client<T> client, MethodCall methodCall, string signature)
        {
            var genericArgs = methodCall.TargetMethod.GetGenericArguments();

            var proxyQuery = new ProxyQuery<T>(signature, methodCall.Args, genericArgs)
            {
                ResultIsIsolated = ResultIsIsolated
            };

            return client.Execute(proxyQuery);
        }
    }
}