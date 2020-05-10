using System;
using System.Collections.Generic;

namespace Memstate
{
    internal class StorageProviders : Providers<StorageProvider>
    {
        //well known storage provider names
        public const string EventStore = nameof(EventStore);
        public const string File = nameof(File);
        public const string Postgres = nameof(Postgres);
        public const string SqlStreamStore = nameof(SqlStreamStore);
        public const string Pravega = nameof(Pravega);


        private const string EventStoreProviderType = "Memstate.EventStore.EventStoreProvider, Memstate.EventStore";
        private const string PostgresProviderType = "Memstate.Postgres.PostgresProvider, Memstate.Postgres";
        private const string SqlStreamStoreProviderType = "Memstate.SqlStreamStore.SqlStreamStoreProvider, Memstate.SqlStreamStore";
        private const string PravegaProviderType = "Memstate.Pravega.PravegaProvider, Memstate.Pravega";

        public StorageProviders()
        {
            Register(File, () => new FileStorageProvider());
            Register(EventStore, () => InstanceFromTypeName(EventStoreProviderType));
            Register(Postgres, () => InstanceFromTypeName(PostgresProviderType));
            Register(SqlStreamStore, () => InstanceFromTypeName(SqlStreamStoreProviderType));
            Register(Pravega, () => InstanceFromTypeName(PravegaProviderType));
        }

        protected override IEnumerable<string> AutoResolutionCandidates()
        {
            yield return "file";
        }
    }
}