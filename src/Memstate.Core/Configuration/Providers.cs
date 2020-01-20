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

        internal protected void UnRegister(string name)
        {
            RegisteredProviders.Remove(name);
        }

        protected static T InstanceFromTypeName(string typeName)
        {
            var type = Type.GetType(typeName, throwOnError: true, ignoreCase: true);
            return (T) Activator.CreateInstance(type);
        }

        protected T AutoResolve()
        {
            var candidates = AutoResolutionCandidates();
            foreach (var candidate in candidates)
            {
                if (TryResolve(candidate, out var provider))
                {
                    LogProvider.GetCurrentClassLogger().Info("Provider resolved: " + provider);
                    return provider;
                }
            }
            //TODO: Add reference to docs for user.
            throw new Exception($"Autoresolve failed for {typeof(T)}. Please check to see if you need to add a reference to 'Memstate.Wire', or 'Memstate.JsonNet'. Adding any of these two nuget packages will automatically use either package for serialisation.");
        }

        internal class Registry : Dictionary<string, Func<T>>
        {
            public Registry() : base(StringComparer.OrdinalIgnoreCase)
            {
            }
        }
    }
}