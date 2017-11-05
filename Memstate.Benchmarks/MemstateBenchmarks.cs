namespace Memstate.Benchmarks
{
    using System;
    using System.Threading.Tasks;

    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Configs;

    using Memstate.EventStore;
    using Memstate.Models;
    using Memstate.Models.KeyValue;
    using Memstate.Postgresql;

    using Microsoft.Extensions.Logging.Console;

    [Config(typeof(MemstateConfig))]
    public class MemstateBenchmarks
    {
        private Engine<KeyValueStore<int>> _engine;
        private Set<int> _setCommand;

        [Params(
            typeof(InMemoryStorageProvider),
            // todo: eventstore hangs, investigate. looks like recordnumber problem
            // typeof(EventStoreProvider),
            typeof(PostgresqlProvider))]
        public Type StorageProviderTypes { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var settings = new MemstateSettings().WithRandomSuffixAppendedToStreamName();

            /* 
            var logProvider = new ConsoleLoggerProvider(filter: (cat, level) => true, includeScopes: false);
            settings.LoggerFactory.AddProvider(logProvider);
            */
            settings.StorageProvider = StorageProviderTypes.AssemblyQualifiedName;

            settings.Configuration["StorageProviders:Postgresql:ConnectionString"] = "Host=localhost; Database=postgres; Password=secret; User ID=postgres;";
            settings.Configuration["StorageProviders:Postgresql:Table"] = $"memstate_commands_{Guid.NewGuid():N}";
            settings.Configuration["StorageProviders:Postgresql:SubscriptionStream"] = $"memstate_notifications_{Guid.NewGuid():N}";

            var engineBuilder = new EngineBuilder(settings);
            _engine = engineBuilder.Build(new KeyValueStore<int>());
            _setCommand = new Set<int>("Key", 42);
        }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            await _engine.DisposeAsync().ConfigureAwait(false);
        }

        [Benchmark]
        public async Task<int> CommandRoundtrip()
        {
            return await _engine.ExecuteAsync(_setCommand).ConfigureAwait(false);
        }

        private class MemstateConfig : ManualConfig
        {
            public MemstateConfig()
            {
                Add(
                    StatisticColumn.Kurtosis,
                    StatisticColumn.OperationsPerSecond,
                    StatisticColumn.P90,
                    StatisticColumn.P95);
            }
        }
    }
}