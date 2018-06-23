using System;
using System.Reflection;
using App.Metrics;

namespace Memstate
{
    public class MemstateSettings : Settings
    {
        public override string Key { get; } = "Memstate";

        public static readonly MemstateSettings Default = new MemstateSettings();

        public static MemstateSettings Current { get; set; } = new MemstateSettings();

        public int MaxBatchSize { get; set; } = 1024;

        public string StreamName { get; set; } = "memstate";

        public string StorageProvider { get; set; } = "file";

        public string Serializer { get; set; } = "Wire";

        public Version Version => GetType().GetTypeInfo().Assembly.GetName().Version;

        public StorageProviders StorageProviders { get; set; } = new StorageProviders();

        public Serializers Serializers { get; set; } = new Serializers();

        public int MaxBatchQueueLength { get; set; } = int.MaxValue;

        public IVirtualFileSystem FileSystem { get; set; } = new HostFileSystem();

        public bool AllowBrokenSequence { get; set; } = false;

        public ISerializer CreateSerializer(string serializer = null) => Serializers.Create(serializer ?? Serializer, this);

        public string Model { get; set; } = typeof(Models.KeyValueStore<int>).AssemblyQualifiedName;

        public string ModelCreator { get; set; } = typeof(DefaultModelCreator).AssemblyQualifiedName;

        public StorageProvider CreateStorageProvider()
        {
            var provider = StorageProviders.Create(StorageProvider, this);
            provider.Initialize();
            return provider;
        }

        public IModelCreator CreateModelCreator()
        {
            var type = Type.GetType(ModelCreator);
            var modelCreator = (IModelCreator) Activator.CreateInstance(type, Array.Empty<object>());
            return modelCreator;
        }

        public override string ToString()
        {
            return $"Provider:{StorageProvider}, Serializer: {Serializer}, Name:{StreamName}";
        }
    }
}