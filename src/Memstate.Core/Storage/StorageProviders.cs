using System;
using System.Collections.Generic;

namespace Memstate
{
    internal class StorageProviders : Providers<StorageProvider>
    {
        private const string EventStoreProviderType = "Memstate.EventStoreProviderType.EventStoreProvider, Memstate.EventStoreProviderType";

        private const string PostgresProviderType = "Memstate.PostgresProviderType.PostgresProvider, Memstate.PostgresProviderType";

        public StorageProviders()
        {
            Register("auto", AutoResolve);
            Register("file", () => new FileStorageProvider());
            Register("EventStore", () => InstanceFromTypeName(EventStoreProviderType));
            Register("Postgres", () => InstanceFromTypeName(PostgresProviderType));
        }

        protected override IEnumerable<string> AutoResolutionCandidates()
        {
            yield return "eventstore";
            yield return "postgres";
            yield return "file";
        }
    }
}