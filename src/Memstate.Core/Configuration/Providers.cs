using System;
using System.Collections.Generic;
using System.Reflection;
using Memstate.Configuration;
using Memstate.Logging;

namespace Memstate
{
    public abstract class Providers<T> where T : class
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

        protected static T InstanceFromTypeName(Config config, string typeName)
        {
            var type = Type.GetType(typeName, throwOnError: true, ignoreCase: true);
            var constructor = type.GetConstructor(new []{typeof(Config)});
            if (constructor is null) throw new Exception("Add a constructor that takes a Config argument");
            return (T) constructor.Invoke(new object[] {config});
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

            var hint = "Please check to see if you need to add a reference to 'Memstate.Wire', or 'Memstate.JsonNet'. Adding any of these two nuget packages will automatically use either package for serialisation.";
            throw new Exception($"Autoresolve failed for {typeof(T)}. {hint}");
        }

        protected class Registry : Dictionary<string, Func<T>>
        {
            public Registry() : base(StringComparer.OrdinalIgnoreCase)
            {
            }
        }
    }
}