namespace Memstate
{
    using System;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class Settings
    {
        // https://msdn.microsoft.com/en-us/magazine/mt632279.aspx
        private readonly IConfiguration _configuration;

        public Settings(string[] args = null)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? Array.Empty<string>())
                .Build()
                .GetSection("Memstate");

            _configuration.Bind(this);
        }

        public Settings(IConfiguration config)
        {
            _configuration = config;
            _configuration.Bind(this);
        }

        public IConfiguration Configuration => _configuration;

        public string StreamName { get; set; } = "memstate";

        public string StorageProvider { get; set; } = "Memstate.FileStorageProvider";

        public string Serializer { get; set; } = "Memstate.Wire.WireSerializerAdapter";

        public Version Version => GetType().GetTypeInfo().Assembly.GetName().Version;

        public ILoggerFactory LoggerFactory { get; } = new LoggerFactory();

        public ISerializer CreateSerializer() => Resolve<ISerializer>(Serializer);

        public StorageProvider CreateStorageProvider() => Resolve<StorageProvider>(StorageProvider);

        /// <summary>
        /// Ensure the configuration is valid or throw an InvalidConfigurationException
        /// </summary>
        public virtual void Validate()
        {
        }

        public ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        private T Resolve<T>(string typeName)
        {
            var type = Type.GetType(typeName, throwOnError: true, ignoreCase: true);
            return (T)Activator.CreateInstance(type, this);
        }
    }
}