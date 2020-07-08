using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using Memstate.Configuration;
using Memstate.EventStore;
using Memstate.Models;
using Memstate.Models.KeyValue;
using Memstate.Postgres;

namespace Memstate.Benchmarks
{
    [Config(typeof(MemstateConfig))]
    public class MemstateBenchmarks
    {
        public const int Iterations = 10;

        private Engine<KeyValueStore<int>> _engine;

        [Params(
            //typeof(InMemoryStorageProvider),
            typeof(PostgresProvider),
            typeof(EventStoreProvider))]
        public Type StorageProviderTypes { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            var config = Config.Current;
            var settings = config.GetSettings<EngineSettings>().WithRandomSuffixAppendedToStreamName();

            /* 
            var logProvider = new ConsoleLoggerProvider(filter: (cat, level) => true, includeScopes: false);
            settings.LoggerFactory.AddProvider(logProvider);
            */
            config.StorageProviderName = StorageProviderTypes.AssemblyQualifiedName;
            config.SerializerName = "newtonsoft.json";
            _engine = await Engine.Start<KeyValueStore<int>>();
        }

        [GlobalCleanup]
        public Task Cleanup()
        {
            return _engine.DisposeAsync();
        }
        
        [Benchmark(OperationsPerInvoke = Iterations)]
        public async Task CommandRoundtrip()
        {
            var tasks = Enumerable
                .Range(0, Iterations)
                .Select(i => _engine.Execute(new Set<int>(i.ToString(), i)));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private class MemstateConfig : ManualConfig
        {
            public MemstateConfig()
            {
                AddColumn(
                    StatisticColumn.Kurtosis,
                    StatisticColumn.OperationsPerSecond,
                    StatisticColumn.P90,
                    StatisticColumn.P95);
            }
        }
    }
}