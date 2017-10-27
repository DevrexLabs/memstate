using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Memstate
{
    public class Settings
    {
        //https://msdn.microsoft.com/en-us/magazine/mt632279.aspx
        private readonly IConfiguration _config;

        public Settings(string[] args = null)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? Array.Empty<string>())
                .Build()
                .GetSection("Memstate");

            _config.Bind(this);
        }

        public Settings(IConfiguration config)
        {
            _config = config;
        }

        public IConfiguration Configuration => _config;

        public string StreamName { get; set; } = "memstate";

        public string Serializer { get; set; } = "wire";

        public Version Version => GetType().GetTypeInfo().Assembly.GetName().Version;

        public ILoggerFactory LoggerFactory { get; } = new LoggerFactory();


        public ISerializer GetSerializer()
        {
            var serializerName = _config["Memstate:Serializer"];
            var typeName = _config["Memstate:Serializers:" + serializerName];
            return (ISerializer)CreateUsingReflection(typeName);
        }

        public ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        public void Bind(object target, string section)
        {
            _config.Bind(target);
        }

        protected object CreateUsingReflection(string typeName)
        {
            var type = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
            return Activator.CreateInstance(type, this);
        }
    }
}