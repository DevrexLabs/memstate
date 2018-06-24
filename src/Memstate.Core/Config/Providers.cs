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

        internal T Resolve(string providerName, MemstateSettings settings)
        {
            return RegisteredProviders.TryGetValue(providerName, out var providerConstructor)
                ? providerConstructor.Invoke(settings)
                : InstanceFromTypeName(providerName, settings);
        }

        protected bool TryResolve(string provider, MemstateSettings settings, out T result)
        {
            try
            {
                result = Resolve(provider, settings);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        protected void Register(string name, Func<MemstateSettings, T> constructor)
        {
            RegisteredProviders[name] = constructor;
        }

        protected static T InstanceFromTypeName(string typeName, MemstateSettings settings)
        {
            var type = Type.GetType(typeName, throwOnError: true, ignoreCase: true);
            return (T) Activator.CreateInstance(type, settings);
        }

        protected T AutoResolve(MemstateSettings settings)
        {
            foreach (var candidate in AutoResolutionCandidates())
            {
                if (TryResolve(candidate, settings, out var provider))
                {
                    LogProvider.GetCurrentClassLogger().Info("Provider resolved: " + provider);
                    return provider;
                }
            }
            throw new Exception("Autoresolve failed for " + typeof(T));
        }

        internal class Registry : Dictionary<string, Func<MemstateSettings, T>>
        {
            public Registry() : base(StringComparer.OrdinalIgnoreCase)
            {
            }
        }
    }
}