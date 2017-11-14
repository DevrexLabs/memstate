namespace System.Test
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Memstate;
    using Memstate.EventStore;
    using Memstate.JsonNet;
    using Memstate.Postgresql;
    using Memstate.Wire;

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

        private IEnumerable<MemstateSettings> GetConfigurations()
        {
            foreach (var serializerName in Serializers())
            {
                foreach (var providerType in ProviderTypes())
                {
                    var settings = new MemstateSettings().WithRandomSuffixAppendedToStreamName();
                    settings.Serializer = serializerName;
                    settings.FileSystem = new InMemoryFileSystem();
                    settings.StorageProvider = providerType.AssemblyQualifiedName;
                    settings.Serializers.Register("newtonsoft.json", _ => new JsonSerializerAdapter(settings));
                    yield return settings;
                }
            }
        }

        private IEnumerable<string> Serializers()
        {
            yield return typeof(JsonSerializerAdapter).AssemblyQualifiedName;
            yield return typeof(WireSerializerAdapter).FullName;
        }

        protected virtual IEnumerable<Type> ProviderTypes()
        {
            //yield return typeof(PostgresqlProvider);
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
                //yield return typeof(PostgresqlProvider);
                yield return typeof(EventStoreProvider);
            }
        }
    }
}