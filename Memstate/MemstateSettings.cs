namespace Memstate
{
    using System;
    using System.Reflection;
    using Microsoft.Extensions.Logging;

    public class MemstateSettings : Settings
    {
        public MemstateSettings(params string[] args) : base("Memstate", args)
        {
        }

        public string StreamName { get; set; } = "memstate";

        public string StorageProvider { get; set; } = "Memstate.FileStorageProvider";

        public string Serializer { get; set; } = "Memstate.Wire.WireSerializerAdapter";

        public Version Version => GetType().GetTypeInfo().Assembly.GetName().Version;

        public ILoggerFactory LoggerFactory { get; } = new LoggerFactory();

        public ISerializer CreateSerializer() => CreateInstanceFromTypeName<ISerializer>(Serializer);

        public StorageProvider CreateStorageProvider() => CreateInstanceFromTypeName<StorageProvider>(StorageProvider);

        /// <summary>
        /// Ensure the configuration is valid or throw an InvalidConfigurationException
        /// </summary>
        public override void Validate()
        {
        }

        public ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        public override string ToString()
        {
            return $"[MemstateSettings -> Name:{StreamName}, Provider:{StorageProvider}, Serializer: {Serializer}]";
        }

        private T CreateInstanceFromTypeName<T>(string typeName)
        {
            var type = Type.GetType(typeName, throwOnError: true, ignoreCase: true);
            return (T)Activator.CreateInstance(type, this);
        }
    }
}