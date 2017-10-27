using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Memstate
{
    public class Config
    {
        //https://msdn.microsoft.com/en-us/magazine/mt632279.aspx?f=255&MSPPError=-2147217396
        private readonly IConfiguration _config;

        public Config(string[] args = null)
        {
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(DefaultConfigurationStrings)
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? Array.Empty<string>())
                .Build();
        }

        public Config(IConfiguration config)
        {
            _config = config;
        }

        public string StreamName
        {
            get => this["StreamName"];
            set => this["StreamName"] = value;
        }

        public Version Version => GetType().GetTypeInfo().Assembly.GetName().Version;

        public string this[string key]
        {
            get => _config[$"Memstate:{key}"];
            set => _config[$"Memstate:{key}"] = value;
        }


        public ISerializer GetSerializer()
        {
            var serializerName = _config["Memstate:Serializer"];
            var typeName = _config["Memstate:Serializers:" + serializerName];
            var type = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
            var serializer = (ISerializer)Activator.CreateInstance(type, this);
            return serializer;
        }

        public ILoggerFactory LoggerFactory { get; } = new LoggerFactory();

        public ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        static IReadOnlyDictionary<string, string> DefaultConfigurationStrings { get; } =
            new Dictionary<string, string>()
            {
                ["Memstate:Serializers:Wire"] = "Memstate.Wire.WireSerializerAdapter",
                ["Memstate:Serializers:Json"] = "Memstate.JsonNet.JsonSerializerAdapter, Memstate.JsonNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                ["Memstate:Serializer"] = "Wire"
            };
    }
}