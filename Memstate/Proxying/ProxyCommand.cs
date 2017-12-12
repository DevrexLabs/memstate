using System;
using System.Reflection;

namespace Memstate
{
    public class ProxyCommand<TModel> : Command<TModel, object>
    {
        public string MethodName { get; set; }

        public object[] Arguments { get; set; }

        public Type[] GenericTypeArguments { get; set; }

        public bool? ResultIsIsolated { get; set; }

        public ProxyCommand(string methodName, object[] inArgs, Type[] genericTypeArguments)
        {
            MethodName = methodName;
            Arguments = inArgs;
            GenericTypeArguments = genericTypeArguments;
        }

        public override object Execute(TModel model, Action<Event> eventHandler)
        {
            try
            {
                var proxyMethod = MethodMap.MapFor<TModel>().GetOperationInfo(MethodName);
                var methodInfo = proxyMethod.MethodInfo;

                if (methodInfo.IsGenericMethod)
                {
                    methodInfo = methodInfo.MakeGenericMethod(GenericTypeArguments);
                }

                return methodInfo.Invoke(model, Arguments);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}