using System;
using System.Collections.Generic;

namespace Memstate
{
    internal class StorageProviders : Providers<StorageProvider>
    {
        //well known storage provider names
        public const string EVENTSTORE = "EventStore";
        public const string FILE = "File";
        public const string POSTGRES = "Postgres";
        public const string SQLSTREAMSTORE = "SqlStreamStore";


        private const string EventStoreProviderType = "Memstate.EventStore.EventStoreProvider, Memstate.EventStore";

        private const string PostgresProviderType = "Memstate.Postgres.PostgresProvider, Memstate.Postgres";

        private const string SqlStreamStoreProviderType = "Memstate.SqlStreamStore.SqlStreamStoreProvider, Memstate.SqlStreamStore";

        public StorageProviders()
        {
            Register(FILE, () => new FileStorageProvider());
            Register(EVENTSTORE, () => InstanceFromTypeName(EventStoreProviderType));
            Register(POSTGRES, () => InstanceFromTypeName(PostgresProviderType));
            Register(SQLSTREAMSTORE, () => InstanceFromTypeName(SqlStreamStoreProviderType));
        }

        protected override IEnumerable<string> AutoResolutionCandidates()
        {
            yield return "file";
        }
    }
}