using System.Reflection;

namespace Memstate
{
    internal sealed class QueryInfo<T> : OperationInfo<T>
    {
        public QueryInfo(MethodInfo methodInfo, OperationAttribute attribute)
            : base(methodInfo, attribute)
        {

        }

        protected override object Execute(Client<T> client, string signature, object query, MethodCall methodCall)
        {
            if (query == null)
            {
                var proxyQuery = new ProxyQuery<T>(signature, methodCall.Args, methodCall.TargetMethod.GetGenericArguments());
                proxyQuery.ResultIsIsolated = ResultIsIsolated;
                query = proxyQuery;
            }
            return client.Execute((Query) query);
        }
    }
}