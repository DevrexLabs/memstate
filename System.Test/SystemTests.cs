namespace System.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Memstate;
    using Memstate.EventStore;
    using Memstate.JsonNet;
    using Memstate.Postgresql;
    using Memstate.Wire;
    using Xunit;
    using Xunit.Abstractions;

    public partial class SystemTests
    {
        private readonly ITestOutputHelper _log;

        public SystemTests(ITestOutputHelper log)
        {
            _log = log;
        }

        public static IEnumerable<object[]> Configurations()
        {
            return GetConfigurations().Select(c => new object[] {c});
        }

        public static IEnumerable<Settings> GetConfigurations()
        {
            foreach (var serializerName in Serializers())
            {
                foreach (var providerType in ProviderTypes())
                {
                    var config = new Settings().WithRandomStreamName();
                    config.Serializer = serializerName;
                    config.StorageProvider = providerType.AssemblyQualifiedName;

                    // NOTE: Not sure if this value should be set here.
                    config.Configuration["StorageProviders:Postgresql:ConnectionString"] = "Host=localhost; Database=postgres; Password=secret; User ID=postgres;";
                    config.Configuration["StorageProviders:Postgresql:Table"] = $"memstate_commands_{Guid.NewGuid():N}";
                    config.Configuration["StorageProviders:Postgresql:SubscriptionStream"] = $"memstate_notifications_{Guid.NewGuid():N}";

                    yield return config;
                }
            }
        }

        public static IEnumerable<object[]> StorageProviders()
        {
            return GetConfigurations()
                .Select(
                    s =>
                    {
                        var provider = s.CreateStorageProvider();

                        provider.Initialize();

                        return provider;
                    })
                .Select(sp => new object[] {sp});
        }

        [Theory]
        [MemberData(nameof(Configurations))]
        public void CanWriteOne(Settings settings)
        {
            var provider = settings.CreateStorageProvider();
            provider.Initialize();
            var writer = provider.CreateJournalWriter(0);

            writer.Send(new AddStringCommand());
            writer.Dispose();

            var reader = provider.CreateJournalReader();
            var records = reader.GetRecords().ToArray();
            reader.Dispose();
            Assert.Equal(1, records.Length);
        }

        [Theory]
        [MemberData(nameof(StorageProviders))]
        public void WriteAndReadCommands(StorageProvider provider)
        {
            provider.Initialize();
            
            var journalWriter = provider.CreateJournalWriter(0);

            for (var i = 0; i < 10000; i++)
            {
                journalWriter.Send(new AddStringCommand());
            }

            journalWriter.Dispose();
            var journalReader = provider.CreateJournalReader();
            var records = journalReader.GetRecords().ToArray();
            journalReader.Dispose();
            Assert.Equal(10000, records.Length);
        }

        [Theory]
        [MemberData(nameof(StorageProviders))]
        public async Task SubscriptionDeliversPreExistingCommands(StorageProvider provider)
        {
            using (provider)
            {
                const int NumRecords = 50;
                var journalWriter = provider.CreateJournalWriter(0);
                for (var i = 0; i < NumRecords; i++)
                {
                    journalWriter.Send(new AddStringCommand());
                }

                journalWriter.Dispose();

                var records = new List<JournalRecord>();
                var subSource = provider.CreateJournalSubscriptionSource();
                subSource.Subscribe(0, records.Add);
                await WaitForConditionOrThrow(() => records.Count == NumRecords).ConfigureAwait(false);
                Assert.Equal(Enumerable.Range(0, NumRecords).ToArray(), records.Select(r => (int) r.RecordNumber).ToArray());
            }
        }

        [Theory]
        [MemberData(nameof(StorageProviders))]
        public async void SubscriptionDeliversFutureCommands(StorageProvider provider)
        {
            const int NumRecords = 5;

            var records = new List<JournalRecord>();
            var subSource = provider.CreateJournalSubscriptionSource();
            var sub = subSource.Subscribe(0, records.Add);
            var writer = provider.CreateJournalWriter(0);

            for (var i = 0; i < NumRecords; i++)
            {
                writer.Send(new AddStringCommand());
            }

            writer.Dispose();
            await WaitForConditionOrThrow(() => records.Count == 5).ConfigureAwait(false);
            sub.Dispose();

            Assert.Equal(5, records.Count);
        }

        [Theory]
        [MemberData(nameof(Configurations))]
        public void Can_execute_void_commands(Settings settings)
        {
            var builder = new EngineBuilder(settings);
            var engine = builder.Build<List<string>>();
            engine.ExecuteAsync(new Reverse());
            engine.Dispose();
        }

        [Theory]
        [MemberData(nameof(Configurations))]
        public async Task Smoke(Settings settings)
        {
            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));

            const int NumRecords = 100;

            var builder = new EngineBuilder(settings);
            var engine = builder.Build<List<string>>();

            foreach (var number in Enumerable.Range(1, NumRecords))
            {
                var command = new AddStringCommand { StringToAdd = number.ToString() };
                var count = await engine.ExecuteAsync(command).ConfigureAwait(false);
                Assert.Equal(number, count);
            }

            engine.Dispose();

            engine = builder.Build<List<string>>();
            var strings = engine.Execute(new GetStringsQuery());
            Assert.Equal(NumRecords, strings.Count);
            engine.Dispose();
        }

        private static IEnumerable<string> Serializers()
        {
            yield return typeof(JsonSerializerAdapter).AssemblyQualifiedName;
            yield return typeof(WireSerializerAdapter).FullName;
        }

        private static IEnumerable<Type> ProviderTypes()
        {
            //todo: broken provider, Engine.Dispose hangs
            //yield return typeof(InMemoryStorageProvider);
            yield return typeof(FileStorageProvider);
            yield return typeof(EventStoreProvider);
            yield return typeof(PostgresqlProvider);
        }

        private async Task WaitForConditionOrThrow(Func<bool> condition, TimeSpan? checkInterval = null, int numberOfTries = 10)
        {
            checkInterval = checkInterval ?? TimeSpan.FromMilliseconds(50);
            while (!condition.Invoke())
            {
                await Task.Delay(checkInterval.Value).ConfigureAwait(false);
                if (numberOfTries-- == 0)
                {
                    throw new TimeoutException();
                }
            }
        }
    }
}