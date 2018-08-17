using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Memstate;
using Memstate.Configuration;
using Memstate.EventStore;
using Memstate.JsonNet;
using Memstate.Postgres;
using Memstate.Wire;

namespace System.Test
{
    public class TestConfigurations : IEnumerable<object[]>
    {
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
                foreach (var providerType in ProviderTypes())
                {
                    var cfg = new Config();
                    cfg.SerializerName = serializerName;
                    cfg.UseInMemoryFileSystem();
                    var settings = cfg.Resolve<MemstateSettings>();
                    settings.WithRandomSuffixAppendedToStreamName();
                    cfg.StorageProviderName = providerType.AssemblyQualifiedName;
                    yield return cfg;
                }
            }
        }

        private IEnumerable<string> Serializers()
        {
            yield return typeof(JsonSerializerAdapter).AssemblyQualifiedName;
            yield return typeof(WireSerializerAdapter).AssemblyQualifiedName;
        }

        protected virtual IEnumerable<Type> ProviderTypes()
        {
            yield return typeof(PostgresProvider);
            yield return typeof(FileStorageProvider);
            yield return typeof(EventStoreProvider);
        }

        private object[] ToObjectArray(object o)
        {
            return new[] {o};
        }

        public class Cluster : TestConfigurations
        {
            protected override IEnumerable<Type> ProviderTypes()
            {
                yield return typeof(EventStoreProvider);
                //yield return typeof(PostgresProvider);
            }
        }

        internal static void Configure(Config config)
        {
            Config.Current = config;
            var settings = config.Resolve<MemstateSettings>();
            settings.WithRandomSuffixAppendedToStreamName();
            Console.WriteLine("C: " + config);
        }
    }
}