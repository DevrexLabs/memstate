using System;
using Microsoft.Extensions.Configuration;

namespace Memstate.Core
{
    public class Config
    {
        private readonly IConfiguration _config;

        public Config(IConfiguration config)
        {
            _config = config;
        }

        public string this[string key]
        {
            get => _config[key];
            set => _config[key] = value;
        }

        public ISerializer GetSerializer()
        {
            var typeName = _config["serializer"];
            var type = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
            var serializer = (ISerializer)Activator.CreateInstance(type);
            return serializer;
        }
    }
}