using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Memstate;
using Memstate.Azure;
using Memstate.Configuration;
using Microsoft.Azure.Cosmos.Table;
using Serilog;
using SqlStreamStore;

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
                foreach (var providerName in ProviderNames())
                {
                    var config = Config.CreateDefault();
                    config.SerializerName = serializerName;
                    config.StorageProviderName = providerName;

                    if (providerName == StorageProviders.SqlStreamStore)
                        ConfigurePgSqlStreamStore(config);

                    if (providerName == StorageProviders.AzureTableStorage)
                        ConfigureAzureTableStorage(config);

                    yield return config;
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
            //yield return "file";
            //yield return "postgres";

            //yield return StorageProviders.EventStore;
            yield return StorageProviders.AzureTableStorage;
            //yield return StorageProviders.SqlStreamStore;
            //yield return "pravega";
        }

        private object[] ToObjectArray(object o)
        {
            return new[] { o };
        }

        private static void ConfigureAzureTableStorage(Config config)
        {
            var tableName = "memstate";
            var connectionString = Environment.GetEnvironmentVariable("AZURE_CLOUDSTORAGE_CONNECTION");
            if (String.IsNullOrEmpty(connectionString)) throw new Exception("AZURE_CLOUDSTORAGE_CONNECTION env variable not set");
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudTableClient();
            var cloudTable = client.GetTableReference(tableName);
            config.UseAzureTableStorage(cloudTable);
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
                //yield return StorageProviders.EventStore;
                yield return StorageProviders.AzureTableStorage;
                //yield return "pravega";
#if POSTGRES
                yield return "postgres";
#endif
            }
        }

    }
}