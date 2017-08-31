using System;
using System.Reflection;

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

        protected override object Execute(Client<T> engine, string signature, object command, MethodCall methodCallMessage)
        {
            if (command == null)
            {
                var proxyCommand = new ProxyCommand<T>(signature, methodCallMessage.Args, methodCallMessage.TargetMethod.GetGenericArguments());
                proxyCommand.ResultIsIsolated = ResultIsIsolated;
                command = proxyCommand;
            }
            var commandHasResult = command.GetType().GenericTypeArguments.Length == 2;
            if (commandHasResult)
            {
                return engine.Execute((Command<T, object>) command);
            }
            else
            {
                engine.Execute((Command<T>) command);
                return null;

            }
        }
    }
}