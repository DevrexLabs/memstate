using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Memstate;
using Memstate.Configuration;
using Serilog;
using SqlStreamStore;

namespace System.Test
{
    public class TestConfigurations : IEnumerable<object[]>
    {
        static TestConfigurations()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}]{Message:lj} {SourceContext}{NewLine}{Exception}")
                .MinimumLevel.Verbose()
                .CreateLogger();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return GetConfigurations()
                .Select(ToObjectArray)
                .GetEnumerator();
        }

        public IEnumerable<Config> GetConfigurations()
        {
            foreach (var serializerName in Serializers())
            {
                foreach (var providerName in ProviderNames())
                {
                    var cfg = Config.Reset();
                    cfg.SerializerName = serializerName;
                    cfg.UseInMemoryFileSystem();
                    var settings = cfg.GetSettings<EngineSettings>();
                    settings.WithRandomSuffixAppendedToStreamName();
                    cfg.StorageProviderName = providerName;
                    if (providerName == "sqlstreamsource")
                        ConfigurePgSqlStreamStore(cfg);
                    yield return cfg;
                }
            }
        }

        private IEnumerable<string> Serializers()
        {
            yield return "newtonsoft.json";
            yield return "wire";
        }

        protected virtual IEnumerable<string> ProviderNames()
        {
            yield return "file";
            //yield return "postgres";

            yield return "eventstore";
            yield return "sqlstreamstore";
            //yield return "pravega";
        }

        private object[] ToObjectArray(object o)
        {
            return new[] { o };
        }

        private static void ConfigurePgSqlStreamStore(Config config)
        {
            var connectionString = "Host=localhost;Port=5432;User Id=postgres;Database=postgres";
            var settings = new PostgresStreamStoreSettings(connectionString);
            var pgStreamStore = new PostgresStreamStore(settings);
            config.Container.Register<IStreamStore>(pgStreamStore);
            pgStreamStore.CreateSchemaIfNotExists().GetAwaiter().GetResult();
        }

        public class Cluster : TestConfigurations
        {
            protected override IEnumerable<string> ProviderNames()
            {
                yield return "eventstore";
                yield return "sqlstreamstore";
                //yield return "pravega";
#if POSTGRES
                yield return "postgres";
#endif
            }
        }

    }
}