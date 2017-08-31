using System;
using System.Reflection;

namespace Memstate
{
    public class ProxyCommand<TModel> : Command<TModel, object>//, IOperationWithResult
    {
        private bool? _resultIsIsolated;

        /// <summary>
        /// Name that uniquely identifies the method to call, including overloads.
        /// Implementation: Obtained by calling TargetMethod.ToString()
        /// Versions prior to 0.19 just the method name.
        /// </summary>
        public string MethodName { get; set; }

        public object[] Arguments { get; set; }

        public Type[] GenericTypeArguments { get; set; }

        public bool? ResultIsIsolated
        {
            get { return _resultIsIsolated; }
            set { _resultIsIsolated = value; }
        }

        public ProxyCommand(string methodName, object[] inArgs, Type[] genericTypeArguments)
        {
            MethodName = methodName;
            Arguments = inArgs;
            GenericTypeArguments = genericTypeArguments;
        }

        public override object Execute(TModel model)
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