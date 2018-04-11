using System;
using System.Collections.Generic;

namespace Memstate
{
    public abstract class Providers<T>
    {
        protected readonly Registry RegisteredProviders = new Registry();

        public T Create(string providerName, MemstateSettings settings)
        {
            return RegisteredProviders.TryGetValue(providerName, out var providerConstructor)
                ? providerConstructor.Invoke(settings)
                : InstanceFromTypeName(providerName, settings);
        }

        public void Register(string name, Func<MemstateSettings, T> constructor)
        {
            RegisteredProviders[name] = constructor;
        }

        private static T InstanceFromTypeName(string typeName, MemstateSettings settings)
        {
            var type = Type.GetType(typeName, throwOnError: true, ignoreCase: true);

            return (T) Activator.CreateInstance(type, settings);
        }

        public class Registry : Dictionary<string, Func<MemstateSettings, T>>
        {
            public Registry() : base(StringComparer.OrdinalIgnoreCase)
            {
            }
        }
    }
}