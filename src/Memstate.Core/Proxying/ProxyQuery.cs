﻿using System;
using System.Reflection;

namespace Memstate
{
    public class ProxyQuery<T> : Query<T, object>
    {
        public ProxyQuery(string methodName, object[] inArgs, Type[] genericTypeArguments)
        {
            MethodName = methodName;
            Arguments = inArgs;
            GenericTypeArguments = genericTypeArguments;
        }

        public string MethodName { get; set; }

        public object[] Arguments { get; set; }

        public Type[] GenericTypeArguments { get; set; }

        public override object Execute(T db)
        {
            try
            {
                var proxyMethod = MethodMap.MapFor<T>().GetOperationInfo(MethodName);
                var method = proxyMethod.MethodInfo;

                if (method.IsGenericMethod)
                {
                    method = method.MakeGenericMethod(GenericTypeArguments);
                }

                return method.Invoke(db, Arguments);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}