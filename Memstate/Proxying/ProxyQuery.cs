using System;
using System.Reflection;

namespace Memstate
{
    public class ProxyQuery<T> : Query<T, object>
    {
        public string MethodName { get; set; }
        public object[] Arguments { get; set; }
        public Type[] GenericTypeArguments { get; set; }


        public ProxyQuery(string methodName, object[] inArgs, Type[] genericTypeArguments)
        {
            MethodName = methodName;
            Arguments = inArgs;
            GenericTypeArguments = genericTypeArguments;
        }

        public override object Execute(T model)
        {
            try
            {
                var proxyMethod = MethodMap.MapFor<T>().GetOperationInfo(MethodName);
                var method = proxyMethod.MethodInfo;

                if (method.IsGenericMethod)
                {
                    method = method.MakeGenericMethod(GenericTypeArguments);
                }
                return method.Invoke(model, Arguments);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}