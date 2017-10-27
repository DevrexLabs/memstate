namespace System.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Memstate;
    using Memstate.EventStore;

    using Xunit;
    using Xunit.Abstractions;

    public class SystemTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SystemTests(ITestOutputHelper log)
        {
            _testOutputHelper = log;
        }

        public static IEnumerable<object[]> GetEngineBuilders()
        {
            foreach (var serializerName in new[] { "Json", "Wire" })
            {
                var config = new Settings();
                config.Serializer = serializerName;
                config.StreamName = "test-" + Guid.NewGuid();
                yield return new object[] { new EventStoreEngineBuilder(config) };

                // todo: yield return new PgsqlProvider()
            }
        }

        public static IEnumerable<object[]> GetConfigurations()
        {
            foreach (var serializerName in new[] { "Json", "Wire" })
            {
                var config = new Settings();
                config.Serializer = serializerName;
                config.StreamName = "test-" + Guid.NewGuid();
                yield return new object[] { new EventStoreProvider(config) };

                // todo: yield return new PgsqlProvider()
            }
        }

        [Theory]
        [MemberData(nameof(GetConfigurations))]
        public void CanWriteOne(Provider provider)
        {
            var writer = provider.CreateJournalWriter();

            writer.Send(new AddStringCommand());
            writer.Dispose();

            var reader = provider.CreateJournalReader();
            var records = reader.GetRecords().ToArray();
            reader.Dispose();
            Assert.Equal(1, records.Length);
        }

        [Theory]
        [MemberData(nameof(GetConfigurations))]
        public void CanWriteMany(Provider provider)
        {
            var journalWriter = provider.CreateJournalWriter();
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
        public async Task SubscriptionFiresEventAppeared(Provider provider)
        {
            using (provider)
            {
                const int NumRecords = 50;
                var journalWriter = provider.CreateJournalWriter();
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

        [Theory]
        [MemberData(nameof(GetConfigurations))]
        public void EventsBatchWrittenAppearOnCatchUpSubscription(Provider provider)
        {
            const int NumRecords = 5;

            // arrange
            var records = new List<JournalRecord>();
            var subSource = provider.CreateJournalSubscriptionSource();
            var sub = subSource.Subscribe(0, records.Add);
            var writer = provider.CreateJournalWriter();

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
        [MemberData(nameof(GetConfigurations))]
        public async Task EventsWrittenAppearOnCatchUpSubscription(Provider provider)
        {
            // Arrange
            var records = new List<JournalRecord>();
            var subSource = provider.CreateJournalSubscriptionSource();
            var sub = subSource.Subscribe(0, records.Add);
            var writer = provider.CreateJournalWriter();

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

        public class Reverse : Command<List<string>>
        {
            public override void Execute(List<string> model)
            {
                model.Reverse();
            }
        }

        [Theory]
        [MemberData(nameof(GetEngineBuilders))]
        public void Can_execute_void_commands(IEngineBuilder builder)
        {
            Engine<List<string>> engine = builder.Build<List<string>>();

            engine.ExecuteAsync(new Reverse());
            engine.Dispose();
        }

        [Theory]
        [MemberData(nameof(GetEngineBuilders))]
        public void Smoke(IEngineBuilder builder)
        {
            const int numRecords = 1;
            var engine = builder.Build<List<string>>();

            var tasks = Enumerable.Range(10, numRecords)
                .Select(n => engine.ExecuteAsync(new AddStringCommand(){StringToAdd = n.ToString()}))
                .ToArray();
            //Task.WaitAll(tasks);
            int expected = 1;
            foreach (var task in tasks) Assert.Equal(expected++, task.Result);
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
    }
}