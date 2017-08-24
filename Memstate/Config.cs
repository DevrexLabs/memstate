using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Memstate
{
    public class Config
    {
        private readonly IConfiguration _config;

        public Config()
        {
        }

        public Config(IConfiguration config)
        {
            _config = config;
        }

        public string this[string key]
        {
            get => _config[$"Memstate:{key}"];
            set => _config[$"Memstate:{key}"] = value;
        }

        public ISerializer GetSerializer()
        {
            var typeName = _config["serializer"];
            var type = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
            var serializer = (ISerializer) Activator.CreateInstance(type);
            return serializer;
        }

        public ILoggerFactory LoggerFactory { get; } = new LoggerFactory();

        public ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}