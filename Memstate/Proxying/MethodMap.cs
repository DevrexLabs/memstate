using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Memstate
{
    /// <summary>
    /// Map method signatures to TargetMethod
    /// </summary>
    internal sealed class MethodMap<T> : MethodMap
    {
        /// <summary>
        /// Proxied methods by signature
        /// </summary>
        private readonly Dictionary<string, OperationInfo<T>> _theMap;


        internal static MethodMap<T> Create(Type modelType)
        {
            var methodMap = new Dictionary<string, OperationInfo<T>>();
            foreach (var methodInfo in modelType.GetRuntimeMethods())
            {
                if (methodInfo.IsPrivate) continue;

                var operationAttribute = GetOperationAttribute(methodInfo);
                var methodName = methodInfo.Name;
                var operationInfo = operationAttribute.ToOperationInfo<T>(methodInfo);
                if (operationAttribute.Type != OperationType.Disallowed) Validate(methodInfo);

                //For backward compatibility, add using just name.
                //Before overload support, only the method name was recorded to the journal.
                //This will support the cases where there are no overloads.
                if (!methodMap.ContainsKey(methodName)) methodMap.Add(methodName, operationInfo);

                // For backward compatibility:
                //Handle the case when overloads are introduced to an existing model. The user can 
                //map a method name in the journal to a specific overload 
                var commandAttribute = operationAttribute as CommandAttribute;
                if (commandAttribute != null && commandAttribute.IsDefault)
                {
                    //ensure there is only one method in the group marked default
                    if (methodMap.ContainsKey(methodName))
                    {
                        var previousOperation = methodMap[methodName].OperationAttribute as CommandAttribute;
                        if (previousOperation != null && previousOperation.IsDefault)
                            throw new Exception("Only one method per group can be marked IsDefault");
                    }

                    methodMap[methodName] = operationInfo;
                }

                //use a unique signature based on the method name and argument types
                var signature = methodInfo.ToString();
                methodMap.Add(signature, operationInfo);
            }

            var result = new MethodMap<T>(methodMap);
            return result;
        }

        private static void Validate(MethodInfo methodInfo)
        {
            if (HasRefArg(methodInfo))
            {
                throw new Exception("ref/out parameters not supported");
            }
        }

        internal static Boolean HasRefArg(MethodInfo methodInfo)
        {
            return methodInfo
                .GetParameters()
                .Any(p => p.ParameterType.IsByRef || p.IsOut);
        }

        internal MethodMap(Dictionary<string, OperationInfo<T>> methodMap)
        {
            _theMap = methodMap;
        }

        private static OperationAttribute GetOperationAttribute(MethodInfo methodInfo)
        {
            //If there is an explicit attribute present, return it
            var attribute = (OperationAttribute)methodInfo
                .GetCustomAttributes(typeof(OperationAttribute), inherit: true)
                .FirstOrDefault();
            if (attribute != null) return attribute;

            var temp = methodInfo.GetCustomAttributes(typeof(NoProxyAttribute), true).FirstOrDefault();
            if (temp != null) attribute = NotAllowedAttribute.Default;

            return attribute ?? GetDefaultOperationAttribute(methodInfo);
        }

        /// <summary>
        /// Void methods are considered as commands, methods with return values are considered queries.
        /// </summary>
        private static OperationAttribute GetDefaultOperationAttribute(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(void)
                ? CommandAttribute.Default
                : QueryAttribute.Default;
        }

        internal OperationInfo<T> GetOperationInfo(string signature)
        {
            return _theMap[signature];
        }
    }

    internal abstract class MethodMap
    {
        private static readonly Dictionary<Type, MethodMap> MethodMaps
            = new Dictionary<Type, MethodMap>();

        internal static MethodMap<T> MapFor<T>()
        {
            Type type = typeof(T);
            if (!MethodMaps.TryGetValue(type, out MethodMap methodMap))
            {
                methodMap = MethodMap<T>.Create(type);
                MethodMaps.Add(type, methodMap);
            }
            return (MethodMap<T>)methodMap;
        }
    }
}