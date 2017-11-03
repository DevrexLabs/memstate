namespace Memstate
{
    using System;
    using System.Reflection;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class MemstateSettings : Settings
    {
        public MemstateSettings(params string[] args) 
            : base(Build(args))
        {
            Memstate = this;
        }

        public string StreamName { get; set; } = "memstate";

        public string StorageProvider { get; set; } = "file";

        public string Serializer { get; set; } = "Wire";

        public Version Version => GetType().GetTypeInfo().Assembly.GetName().Version;

        public ILoggerFactory LoggerFactory { get; } = new LoggerFactory();

        public StorageProviders StorageProviders { get; set; } = new StorageProviders();

        public Serializers Serializers { get; set; } = new Serializers();

        public ISerializer CreateSerializer(string serializer = null) => Serializers.Create(serializer ?? Serializer, this);

        public StorageProvider CreateStorageProvider() => StorageProviders.Create(StorageProvider, this);


        public ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        public override string ToString()
        {
            return $"[Provider:{StorageProvider}, Serializer: {Serializer}, Name:{StreamName}]";
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