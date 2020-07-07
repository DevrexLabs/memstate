using System;
using System.Collections.Generic;
using System.Reflection;
using TinyIoC;
using System.Text;
using Fig;

namespace Memstate.Configuration
{

    public sealed class Config
    {
        /// <summary>
        /// Underlying Fig settings
        /// </summary>
        public Fig.Settings ConfigurationData { get; }

        /// <summary>
        /// Rebuilds the <see cref="Current"/> config from inputs,
        /// discarding any modifications or cached settings objects.
        /// </summary> 
        /// <returns>The newly reset Current config</returns>
        internal static Config Reset()
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
        /// We want everything to be singleton by default unless explicitly requested/registered 
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
                if (_current == null)
                {
                    lock (_lock)
                    {
                        if (_current == null) _current = BuildDefault();
                    }
                }
                
                return _current;
            }
            internal set
            {
                lock(_lock) _current = value;
            }
        }

        /// <summary>
        /// Helper method to set the <see cref="Config.FileSystem"/> 
        /// property with a new instance of <see cref="InMemoryFileSystem"/>
        /// This is useful for test when using File storage
        /// </summary>
        public Config UseInMemoryFileSystem()
        {
            FileSystem = new InMemoryFileSystem();
            return this;
        }

        /// <summary>
        /// Version of the Memstate.Core library.
        /// Always corresponds to the release version on GH and nuget
        /// </summary>
        /// <value>The version.</value>
        public Version Version => GetType().GetTypeInfo().Assembly.GetName().Version;

        /// <summary>
        /// Configurable file system, defaults to file system on the host.
        /// Useful for testing, see <see cref="InMemoryFileSystem"/>.
        /// </summary>
        /// <value>The file system.</value>
        public IFileSystem FileSystem { get; set; } = new HostFileSystem();

        public ISerializer CreateSerializer(string serializer = null) 
            => Serializers.Resolve(serializer ?? SerializerName);

        public static Config BuildDefault(string[] args = null)
        { 
            args = args ?? Environment.GetCommandLineArgs();

            var settings = new SettingsBuilder()
                .UseCommandLine(args)
                .UseEnvironmentVariables("MEMSTATE_", dropPrefix: false)
                .UseIniFile("memstate.${ENV}.ini", required: false)
                .UseIniFile("memstate.ini", required: false)
                .Build();

            return new Config(settings);
        }
        
        public Config(Fig.Settings settings)
        {
            ConfigurationData = settings;
            ConfigurationData.Bind(this, requireAll:false, prefix:"Memstate");
            Container = new TinyIoCContainer();
            Container.Register(this);
        }

        /// <summary>
        /// Get a singleton reference to a <see cref="ConfigurationData"/> object which has been configured with values from 
        /// the underlying configuration parameters
        /// </summary>
        public T GetSettings<T>() where T : Settings, new()
        {
            if (_singletonCache.TryGetValue(typeof(T), out object result))
            {
                return (T) result;
            }
            else
            {
                var instance = new T();
                ConfigurationData.Bind(instance, prefix: instance.BindingPath, requireAll: false);
                _singletonCache[typeof(T)] = instance;
                return instance;
            }
        }
        
        /// <summary>
        /// Assign a storage provider instance or leave null and it will
        /// be assigned automatically based on the value of StorageProviderName
        /// </summary>
        public void SetStorageProvider(StorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Name of a well known storage provider OR resolvable type name,
        /// default is File which uses the local file system
        /// </summary>
        public string StorageProviderName { get; set; } = StorageProviders.File;

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

        /// <summary>
        /// Name of a well known serializer or resolvable type name OR the value Auto (default)
        /// </summary>
        /// <value>The name of the serializer.</value>
        public string SerializerName { get; set; } = Serializers.Auto;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("-- CONFIG --");
            builder.AppendLine(nameof(SerializerName) + "=" + SerializerName);
            builder.AppendLine(nameof(StorageProviderName) + "=" + StorageProviderName);
            builder.AppendLine(nameof(FileSystem) + "=" + FileSystem);
            builder.AppendLine(nameof(Version) + "=" + Version);

            builder.AppendLine("-- DATA --");
            try
            {
                builder.Append(ConfigurationData);
            }
            catch (Exception ex)
            {
                builder.Append("No data");
            }
            return builder.ToString();
        }
    }    
}
