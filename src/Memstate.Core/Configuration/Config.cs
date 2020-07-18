using System;
using System.Collections.Generic;
using System.Reflection;
using TinyIoC;
using Fig;
using Memstate.Logging;
using Memstate.Logging.LogProviders;

namespace Memstate.Configuration
{

    public class Config
    {
        /// <summary>
        /// Underlying Fig settings
        /// </summary>
        public Fig.Settings ConfigurationData { get; }

        internal TinyIoCContainer Container { get; }
        
        private IStorageProvider _storageProvider;

        /// <summary>
        /// We want everything to be singleton by default unless explicitly requested/registered 
        /// </summary>
        private readonly Dictionary<Type, object> _singletonCache
            = new Dictionary<Type, object>();
        
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

        public static Config CreateDefault(string[] args = null)
        {
            #if TEST
            LogProvider.SetCurrentLogProvider(new ColoredConsoleLogProvider());
            #endif
            args = args ?? Environment.GetCommandLineArgs();

            var settings = new SettingsBuilder()
                .UseCommandLine(args)
                .UseEnvironmentVariables("MEMSTATE_", dropPrefix: false)
                .UseIniFile("memstate.${ENV}.ini", required: false)
                .UseIniFile("memstate.ini", required: false)
                .Build();
            var config = new Config(settings);
            
            #if DEBUG
            config.GetSettings<EngineSettings>()
                .WithRandomSuffixAppendedToStreamName();
            config.UseInMemoryFileSystem();
            #endif
            
            return config;
        }
        
        public Config(Fig.Settings settings)
        {
            ConfigurationData = settings;
            ConfigurationData.Bind(this, requireAll:false, prefix:"Memstate");
            Container = new TinyIoCContainer();
            StorageProviders = new StorageProviders(this);
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
        public void SetStorageProvider(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        /// Name of a well known storage provider OR resolvable type name,
        /// default is File which uses the local file system
        /// </summary>
        public string StorageProviderName { get; set; } = StorageProviders.File;

        public IStorageProvider GetStorageProvider()
        {
            if (_storageProvider == null)
            {
                _storageProvider = StorageProviders.Resolve(StorageProviderName);
                _storageProvider.Provision().GetAwaiter().GetResult();
            }
            return _storageProvider;
        }

        internal StorageProviders StorageProviders { get; set; }

        internal Serializers Serializers { get; set; }
            = new Serializers();

        /// <summary>
        /// Name of a well known serializer or resolvable type name OR the value Auto (default)
        /// </summary>
        /// <value>The name of the serializer.</value>
        public string SerializerName { get; set; } = Serializers.Auto;

        public override string ToString()
        {
            return SerializerName + "," + StorageProviderName;
        }
    }    
}
