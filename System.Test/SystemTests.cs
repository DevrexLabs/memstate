namespace System.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Memstate;
    using Memstate.EventStore;
    using Memstate.JsonNet;
    using Memstate.Postgresql;
    using Memstate.Wire;

    using Xunit;
    using Xunit.Abstractions;

    public class SystemTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SystemTests(ITestOutputHelper log)
        {
            _testOutputHelper = log;
        }

        public static IEnumerable<object[]> Configurations()
        {
            return GetConfigurations().Select(c => new object[] { c });
        } 

        public static IEnumerable<Settings> GetConfigurations()
        {
            foreach (var serializerName in Serializers())
            {
                var config = new Settings().WithRandomStreamName().WithInmemoryStorage();
                config.Serializer = serializerName;
                yield return config;

                config.StorageProvider = typeof(EventStoreProvider).AssemblyQualifiedName;
                yield return config;

                config.StorageProvider = typeof(FileStorageProvider).FullName;
                yield return config;

                config.StorageProvider = typeof(PostgresqlProvider).AssemblyQualifiedName;
                yield return config;
            }
        }

        public static IEnumerable<object[]> StorageProviders()
        {
            return GetConfigurations()
                .Select(s => s.CreateStorageProvider())
                .Select(sp => new object[] { sp });
        }

        [Theory]
        [MemberData(nameof(Configurations))]
        public void CanWriteOne(Settings settings)
        {
            var provider = settings.CreateStorageProvider();
            var writer = provider.CreateJournalWriter(1);

            writer.Send(new AddStringCommand());
            writer.Dispose();

            var reader = provider.CreateJournalReader();
            var records = reader.GetRecords().ToArray();
            reader.Dispose();
            Assert.Equal(1, records.Length);
        }

        [Theory]
        [MemberData(nameof(StorageProviders))]
        public void CanWriteMany(StorageProvider provider)
        {
            var journalWriter = provider.CreateJournalWriter(1);
            for(var i = 0; i < 10000; i++)
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
        public async Task SubscriptionFiresEventAppeared(StorageProvider provider)
        {
            using (provider)
            {
                const int NumRecords = 50;
                var journalWriter = provider.CreateJournalWriter(1);
                for (var i = 0; i < NumRecords; i++)
                {
                    journalWriter.Send(new AddStringCommand());
                }
                journalWriter.Dispose();

                var records = new List<JournalRecord>();
                var subSource = provider.CreateJournalSubscriptionSource();
                var subscription = subSource.Subscribe(0, records.Add);
                await WaitForConditionOrThrow(() => records.Count == NumRecords).ConfigureAwait(false);
                Assert.Equal(Enumerable.Range(0, NumRecords), records.Select(r => (int)r.RecordNumber));
            }
        }

        [Theory]
        [MemberData(nameof(StorageProviders))]
        public void EventsBatchWrittenAppearOnCatchUpSubscription(StorageProvider provider)
        {
            const int NumRecords = 5;

            // arrange
            var records = new List<JournalRecord>();
            var subSource = provider.CreateJournalSubscriptionSource();
            var sub = subSource.Subscribe(0, records.Add);
            var writer = provider.CreateJournalWriter(1);

            // act
            for (int i = 0; i < NumRecords; i++)
            {
                writer.Send(new AddStringCommand());
            }
            writer.Dispose();
            while(records.Count < 5) Thread.Sleep(0);
            sub.Dispose();

            Assert.Equal(5, records.Count);
        }

        [Theory]
        [MemberData(nameof(Configurations))]
        public async Task EventsWrittenAppearOnCatchUpSubscription(StorageProvider provider)
        {
            // Arrange
            var records = new List<JournalRecord>();
            var subSource = provider.CreateJournalSubscriptionSource();
            var sub = subSource.Subscribe(0, records.Add);
            var writer = provider.CreateJournalWriter(1);

            // Act
            writer.Send(new AddStringCommand());
            writer.Send(new AddStringCommand());
            writer.Send(new AddStringCommand());
            writer.Send(new AddStringCommand());
            writer.Send(new AddStringCommand());
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
            const int numRecords = 1;

            var builder = new EngineBuilder(settings);
            var engine = builder.Build<List<string>>();

            var tasks = Enumerable.Range(10, numRecords)
                .Select(n => engine.ExecuteAsync(new AddStringCommand(){StringToAdd = n.ToString()}))
                .ToArray();
            int expected = 1;
            foreach (var task in tasks)
            {
                Assert.Equal(expected++, await task.ConfigureAwait(false));
            }
            //foreach (var number in Enumerable.Range(1,100))
            //{
            //    var command = new AddStringCommand() {StringToAdd = number.ToString()};
            //    var count = await engine.ExecuteAsync(command);
            //    _log.WriteLine("executed " + number);
            //    Assert.Equal(number, count);
            //}

            engine.Dispose();

            //is the builder reusable?
            //can we load when there are existing commands in the stream
            engine = builder.Build<List<string>>();
            var strings = engine.Execute(new GetStringsQuery());
            Assert.Equal(numRecords, strings.Count);
            engine.Dispose();
        }

        private static IEnumerable<string> Serializers()
        {
            yield return typeof(JsonSerializerAdapter).FullName;
            yield return typeof(WireSerializerAdapter).FullName;
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

        public class Reverse : Command<List<string>>
        {
            public override void Execute(List<string> model)
            {
                model.Reverse();
            }
        }
    }
}