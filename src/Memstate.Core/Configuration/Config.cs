using System;
using System.Collections.Generic;
using System.Reflection;
using TinyIoC;
using System.Text;

namespace Memstate.Configuration
{
    public sealed class Config
    {



        /// <summary>
        /// The underlying key value pairs
        /// </summary>
        internal Dictionary<string, string> Data { get; }

        /// <summary>
        /// Rebuilds the <see cref="Current"/> config from inputs,
        /// discarding any modifications or cached settings objects. 
        /// <returns>The newly reset Current config</returns>
        public static Config Reset()
        {
            Current = null;
            return Current;
        }

        internal TinyIoCContainer Container { get; }

        /// <summary>
        /// Backing field of the Config.Current property
        /// </summary>
        private static Config _current;

        private StorageProvider _storageProvider;

        /// <summary>
        /// We want everything to be singleton by default unless explictly requested/registered 
        /// </summary>
        private readonly Dictionary<Type, object> _singletonCache
            = new Dictionary<Type, object>();


        /// <summary>
        /// Synchronization object for the Config.Current field
        /// </summary>
        private static readonly object _lock = new object();

        public static Config Current
        {
            get
            {
                lock (_lock)
                {
                    if (_current == null) _current = BuildDefault();
                }
                return _current;
            }
            internal set
            {
                lock(_lock) _current = value;
            }
        }

        //todo: move to an extension method in a Test namespace
        public void UseInMemoryFileSystem()
        {
            FileSystem = new InMemoryFileSystem();    
        }

        public Version Version => GetType().GetTypeInfo().Assembly.GetName().Version;

        public IFileSystem FileSystem { get; set; } = new HostFileSystem();

        public ISerializer CreateSerializer(string serializer = null) => Serializers.Resolve(serializer ?? SerializerName);

        private static Config BuildDefault()
        {
            //todo: add support for a config file. INI perhaps ???
            var args = Environment.GetCommandLineArgs();

            var config = new ConfigBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            return config;
        }

        public Config(Dictionary<string, string> args = null)
        {
            args = args ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Data = args;
            Container = new TinyIoCContainer();
            Container.Register(this);
        }

        public T GetSettings<T>() where T : Settings
        {
            if (_singletonCache.TryGetValue(typeof(T), out object result))
            {
                return (T)result;
            }
            else
            {
                var newInstance = Container.Resolve<T>();
                _singletonCache[typeof(T)] = newInstance;
                Bind(newInstance, newInstance.Key);
                return newInstance;
            }
        }

        /// <summary>
        /// Copy data from the configuration to matching public 
        /// properties on the object
        /// </summary>
        public void Bind(Object @object, string prefix)
        {
            foreach(var property in @object.GetType().GetProperties())
            {
                var candidateKey = prefix + ":" + property.Name;
                if (Data.TryGetValue(candidateKey, out string value))
                {
                    Console.WriteLine("Bind (" + candidateKey + ") = (" + value + ")");
                    property.SetValue(@object, Convert(value, property.PropertyType));
                }
            }
        }


        /// <summary>
        /// Assign a storage provider or leave null and it will
        /// be assigned automatically based on the value of StorageProviderName
        /// </summary>
        public void SetStorageProvider(StorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Name of a well known storage provider OR resolvable type name
        /// OR the literal "Auto" (which is default) for automatic resolution.
        /// Automatic resolution will take the first available of eventstore,
        /// postgres or filestorage.
        /// </summary>
        public string StorageProviderName { get; set; } = "Auto";

        public StorageProvider GetStorageProvider()
        {
            if (_storageProvider == null)
            {
                _storageProvider = StorageProviders.Resolve(StorageProviderName);
                _storageProvider.Initialize();
            }
            return _storageProvider;
        }

        internal StorageProviders StorageProviders { get; set; }
            = new StorageProviders();

        internal Serializers Serializers { get; set; }
            = new Serializers();

        public string SerializerName { get; set; } = "Auto";

        private object Convert(string value, Type type)
        {
            if (Converters.TryGetValue(type, out var converter))
            {
                return converter.Invoke(value);
            }
            else throw new NotImplementedException("Conversion to type " + type.FullName + " not supported");
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("-- CONFIG --");
            builder.AppendLine(nameof(SerializerName) + "=" + SerializerName);
            builder.AppendLine(nameof(StorageProviderName) + "=" + StorageProviderName);
            builder.AppendLine(nameof(FileSystem) + "=" + FileSystem);
            builder.AppendLine(nameof(Version) + "=" + Version);

            builder.AppendLine("-- DATA --");
            foreach(var key in Data.Keys)
            {
                builder.AppendLine(key + "=" + Data[key]);
            }
            return builder.ToString();

        }

        public static readonly Dictionary<Type, Func<string, object>> Converters
            = new Dictionary<Type, Func<string, object>>()
            {
                {typeof(Int32), s => Int32.Parse(s)},
                {typeof(Int64), s => Int64.Parse(s)},
                {typeof(bool), s => bool.Parse(s)},
                {typeof(string), s => s}
            };
    }    
}
