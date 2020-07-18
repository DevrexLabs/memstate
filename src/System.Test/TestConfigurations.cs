using System.Collections.Generic;
using System.Linq;
using Memstate;
using Memstate.Azure;
using Memstate.Configuration;
using Microsoft.Azure.Cosmos.Table;
using SqlStreamStore;

namespace System.Test
{
    public static class TestConfigurations
    {

        public static IEnumerator<object[]> All()
        {
            return GetConfigurations()
                .Select(ToObjectArray)
                .GetEnumerator();
        }

        public static IEnumerator<object[]> Cluster()
        {
            return GetConfigurations()
                .Where(c => c.StorageProviderName != StorageProviders.File)
                .Select(ToObjectArray)
                .GetEnumerator();
        }
        public static IEnumerable<Config> GetConfigurations()
        {
            foreach (var serializerName in GetSerializers())
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

        private static IEnumerable<string> GetSerializers()
        {
            yield return Serializers.Wire;
            yield return Serializers.NewtonsoftJson;
            yield return Serializers.BinaryFormatter;
        }

        private static IEnumerable<string> ProviderNames()
        {
            yield return StorageProviders.File;
            yield return StorageProviders.Postgres;

            yield return StorageProviders.EventStore;
            yield return StorageProviders.AzureTableStorage;
            //yield return StorageProviders.SqlStreamStore;
            //yield return StorageProviders.Pravega;
        }

        private static object[] ToObjectArray(object o)
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
    }
}