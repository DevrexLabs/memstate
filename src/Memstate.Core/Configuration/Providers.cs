using System;
using System.Collections.Generic;
using Memstate.Logging;

namespace Memstate
{
    internal abstract class Providers<T> where T : class
    {
        protected readonly Registry RegisteredProviders = new Registry();

        /// <summary>
        /// List of provider names to try during auto resolution, see AutoResolve
        /// </summary>
        protected abstract IEnumerable<string> AutoResolutionCandidates();

        internal T Resolve(string providerName)
        {
            providerName = providerName ?? "Auto";
            return RegisteredProviders.TryGetValue(providerName, out var providerConstructor)
                ? providerConstructor.Invoke()
                : InstanceFromTypeName(providerName);
        }

        protected bool TryResolve(string provider, out T result)
        {
            try
            {
                result = Resolve(provider);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        protected void Register(string name, Func<T> constructor)
        {
            RegisteredProviders[name] = constructor;
        }

        protected static T InstanceFromTypeName(string typeName)
        {
            var type = Type.GetType(typeName, throwOnError: true, ignoreCase: true);
            return (T) Activator.CreateInstance(type);
        }

        protected T AutoResolve()
        {
            foreach (var candidate in AutoResolutionCandidates())
            {
                if (TryResolve(candidate, out var provider))
                {
                    LogProvider.GetCurrentClassLogger().Info("Provider resolved: " + provider);
                    return provider;
                }
            }
            throw new Exception("Autoresolve failed for " + typeof(T));
        }

        internal class Registry : Dictionary<string, Func<T>>
        {
            public Registry() : base(StringComparer.OrdinalIgnoreCase)
            {
            }
        }
    }
}