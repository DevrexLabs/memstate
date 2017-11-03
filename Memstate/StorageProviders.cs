namespace Memstate
{
    using System;
    using System.Collections.Generic;

    public static class StorageProviders
    {
        private static readonly Registry RegisteredProviders;

        static StorageProviders()
        {
            RegisteredProviders = new Registry();
            Register("FileStorage", settings => new FileStorageProvider(settings));
            Register("InmemoryStorage", settings => new InMemoryStorageProvider(settings));
        }

        public static StorageProvider Create(MemstateSettings settings)
        {
            var providerName = settings.StorageProvider;
            if (RegisteredProviders.TryGetValue(providerName, out var providerConstructor))
            {
                return providerConstructor.Invoke(settings);
            }

            throw new ArgumentException("Unrecognized StorageProvider", providerName);
        }

        public static void Register(string name, Func<MemstateSettings, StorageProvider> constructor)
        {
            RegisteredProviders[name] = constructor;
        }

        private class Registry : Dictionary<string, Func<MemstateSettings, StorageProvider>>
        {
            public Registry() : base(StringComparer.OrdinalIgnoreCase)
            {
            }
        }
    }
}