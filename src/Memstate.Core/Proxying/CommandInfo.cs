using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Memstate
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class OperationAttribute : Attribute
    {
        internal OperationType Type { get; set; }

        /// <summary>
        /// Isolation guarantees of this operation
        /// </summary>
        public IsolationLevel Isolation { get; set; }

        /// <summary>
        /// Map to an explict Command or Query type or the generic proxy types if null
        /// </summary>
        public Type MapTo { get; set; }

        internal abstract OperationInfo<T> ToOperationInfo<T>(MethodInfo methodInfo);
    }

    internal sealed class CommandInfo<T> : OperationInfo<T>
    {
        public CommandInfo(MethodInfo methodInfo, OperationAttribute attribute)
            : base(methodInfo, attribute)
        {
        }

        protected override Task<object> ExecuteProxy(Client<T> engine, MethodCall methodCall, string signature)
        {
            var genericArgs = methodCall.TargetMethod.GetGenericArguments();
            var proxyCommand = new ProxyCommand<T>(signature, methodCall.Args, genericArgs);
            proxyCommand.ResultIsIsolated = ResultIsIsolated;
            return engine.Execute(proxyCommand);
        }

        protected async override Task<object> ExecuteMapped(Client<T> engine, MethodCall methodCallMessage, object mappedCommand)
        {
            //Command<TModel>.Execute is void
            //Command<TModel,TResult>.Execute returns TResult 
            var commandHasResult = mappedCommand.GetType().GenericTypeArguments.Length == 2;
            if (commandHasResult)
            {
                return engine.Execute((Command<T, object>) mappedCommand);
            }
            await engine.Execute((Command<T>) mappedCommand).NotOnCapturedContext();
            return Task.FromResult<object>(null);
        }
    }
}