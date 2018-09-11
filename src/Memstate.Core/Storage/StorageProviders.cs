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


        private const string EventStoreProviderType = "Memstate.EventStore.EventStoreProvider, Memstate.EventStore";

        private const string PostgresProviderType = "Memstate.Postgres.PostgresProvider, Memstate.Postgres";

        public StorageProviders()
        {
            Register(FILE, () => new FileStorageProvider());
            Register(EVENTSTORE, () => InstanceFromTypeName(EventStoreProviderType));
            Register(POSTGRES, () => InstanceFromTypeName(PostgresProviderType));
        }

        protected override IEnumerable<string> AutoResolutionCandidates()
        {
            yield return "file";
        }
    }
}