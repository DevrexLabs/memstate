using System;
using System.Collections.Generic;
using Memstate.Configuration;

namespace Memstate
{
    internal class StorageProviders : Providers<IStorageProvider>
    {
        //well known storage provider names
        public const string EventStore = nameof(EventStore);
        public const string File = nameof(File);
        public const string Postgres = nameof(Postgres);
        public const string SqlStreamStore = nameof(SqlStreamStore);
        public const string Pravega = nameof(Pravega);
        public const string AzureTableStorage = nameof(AzureTableStorage);


        private static readonly Dictionary<string, string> TypeNames = new Dictionary<string, string>()
        {
            {EventStore, "Memstate.EventStore.EventStoreProvider, Memstate.EventStore"},
            {Postgres, "Memstate.Postgres.PostgresProvider, Memstate.Postgres"},
            {SqlStreamStore, "Memstate.SqlStreamStore.SqlStreamStoreProvider, Memstate.SqlStreamStore"},
            {Pravega, "Memstate.Pravega.PravegaProvider, Memstate.Pravega"},
            {AzureTableStorage, "Memstate.Azure.TableStorageProvider, Memstate.Azure"}
        };

        public StorageProviders(Config config)
        {
            Register(File, () => new FileStorageProvider(config));
            foreach (var keyValuePair in TypeNames)
            {
                Register(keyValuePair.Key, () => InstanceFromTypeName(config, keyValuePair.Value));
            }
        }

        protected override IEnumerable<string> AutoResolutionCandidates()
        {
            yield return File;
        }
    }
}