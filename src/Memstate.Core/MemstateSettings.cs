using System;
using System.Reflection;
using App.Metrics;
using App.Metrics.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Memstate.Logging;


namespace Memstate
{
    public class MemstateSettings : Settings
    {
        public MemstateSettings(params string[] args)
            : base(Build(args))
        {
            Memstate = this;

            Metrics = new MetricsBuilder()
                .Configuration.ReadFrom(Configuration)
                .OutputMetrics.AsJson()
                .Build();
        }

        public IMetricsRoot Metrics { get; }

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

        private static IConfiguration Build(params string[] commandLineArguments)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(commandLineArguments ?? Array.Empty<string>())
                .Build()
                .GetSection("Memstate");
        }
    }
}