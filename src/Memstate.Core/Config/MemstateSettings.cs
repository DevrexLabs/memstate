using System;
using System.Reflection;

namespace Memstate
{
    public class MemstateSettings : Settings
    {
        public override string Key { get; } = "Memstate";

        public static MemstateSettings Current { get; set; }

        public static void Initialize()
        {
            if (Current != null) return;
            Current = Settings.Get<MemstateSettings>();
        }

        /// <summary>
        /// Maximum number of commands per batch sent to journal writer
        /// </summary>
        public int MaxBatchSize { get; set; } = 1024;

        public string StreamName { get; set; } = "memstate";

        /// <summary>
        /// Name of a well known storage provider OR resolvable type name
        /// OR the literal "Auto" (which is default) for automatic resolution.
        /// Automatic resolution will take the first available of eventstore,
        /// postgres or filestorage.
        /// </summary>
        public string StorageProviderName { get; set; } = "Auto";
        
        public string SerializerName { get; set; } = "Auto";

        public Version Version => GetType().GetTypeInfo().Assembly.GetName().Version;

        private StorageProvider _storageProvider;

        /// <summary>
        /// Assign a storage provider or leave null and it will
        /// be assigned automatically based on the value of StorageProviderName
        /// </summary>
        public void SetStorageProvider(StorageProvider storageProvider)
        {
             _storageProvider = storageProvider;
        }
    

        internal StorageProviders StorageProviders { get; set; }
            = new StorageProviders();

        internal Serializers Serializers { get; set; }
            = new Serializers();

        public int MaxBatchQueueLength { get; set; } = int.MaxValue;

        public IVirtualFileSystem FileSystem { get; set; } = new HostFileSystem();

        public bool AllowBrokenSequence { get; set; } = false;

        public ISerializer CreateSerializer(string serializer = null) => Serializers.Resolve(serializer ?? SerializerName, this);

        public string Model { get; set; } = typeof(Models.KeyValueStore<int>).AssemblyQualifiedName;

        public string ModelCreator { get; set; } = typeof(DefaultModelCreator).AssemblyQualifiedName;

        public StorageProvider GetStorageProvider()
        {
            if (_storageProvider == null)
            {
                _storageProvider = StorageProviders.Resolve(StorageProviderName, this);
                _storageProvider.Initialize();
            }
            return _storageProvider;
        }

        public IModelCreator CreateModelCreator()
        {
            var type = Type.GetType(ModelCreator);
            var modelCreator = (IModelCreator) Activator.CreateInstance(type, Array.Empty<object>());
            return modelCreator;
        }

        public override string ToString()
        {
            return $"Provider:{StorageProviderName}, SerializerName: {SerializerName}, Name:{StreamName}";
        }
    }
}