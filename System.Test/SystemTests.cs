namespace System.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Memstate;

    using Xunit;
    using Xunit.Abstractions;

    public class SystemTests
    {
        private readonly ITestOutputHelper _log;
        private readonly string _randomStreamName;

        public SystemTests(ITestOutputHelper log)
        {
            _log = log;
            _randomStreamName = "memstate" + Guid.NewGuid().ToString("N").Substring(0, 10);
        }

        [Theory]
        [ClassData(typeof(TestConfigurations))]
        public void CanWriteOne(MemstateSettings settings)
        {
            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;
            _log.WriteLine(settings.ToString());

            using (var provider = settings.CreateStorageProvider())
            {
                provider.Initialize();
                var writer = provider.CreateJournalWriter(0);

                writer.Send(new AddStringCommand("hello"));
                writer.Dispose();

                var reader = provider.CreateJournalReader();
                var records = reader.GetRecords().ToArray();
                reader.Dispose();
                Assert.Equal(1, records.Length);
            }
        }

        [Theory]
        [ClassData(typeof(TestConfigurations))]
        public void WriteAndReadCommands(MemstateSettings settings)
        {
            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;

            using (var provider = settings.CreateStorageProvider())
            {
                provider.Initialize();

                var journalWriter = provider.CreateJournalWriter(0);

                for (var i = 0; i < 10000; i++)
                {
                    journalWriter.Send(new AddStringCommand(i.ToString()));
                }

                journalWriter.Dispose();
                var journalReader = provider.CreateJournalReader();
                var records = journalReader.GetRecords().ToArray();
                journalReader.Dispose();
                Assert.Equal(10000, records.Length);
            }
        }

        [Theory]
        [ClassData(typeof(TestConfigurations))]
        public async Task SubscriptionDeliversPreExistingCommands(MemstateSettings settings)
        {
            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;

            using (var provider = settings.CreateStorageProvider())
            {
                const int NumRecords = 50;
                var journalWriter = provider.CreateJournalWriter(0);
                for (var i = 0; i < NumRecords; i++)
                {
                    journalWriter.Send(new AddStringCommand(i.ToString()));
                }

                journalWriter.Dispose();

                var records = new List<JournalRecord>();
                var subSource = provider.CreateJournalSubscriptionSource();
                subSource.Subscribe(0, records.Add);
                await WaitForConditionOrThrow(() => records.Count == NumRecords).ConfigureAwait(false);
                Assert.Equal(Enumerable.Range(0, NumRecords).ToArray(), records.Select(r => (int)r.RecordNumber).ToArray());
            }
        }

        [Theory]
        [ClassData(typeof(TestConfigurations))]
        public async Task SubscriptionDeliversFutureCommands(MemstateSettings settings)
        {
            const int NumRecords = 5;

            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;

            using (var provider = settings.CreateStorageProvider())
            {
                var records = new List<JournalRecord>();
                var subSource = provider.CreateJournalSubscriptionSource();
                var sub = subSource.Subscribe(0, records.Add);
                var writer = provider.CreateJournalWriter(0);

                for (var i = 0; i < NumRecords; i++)
                {
                    writer.Send(new AddStringCommand(i.ToString()));
                }

                writer.Dispose();
                await WaitForConditionOrThrow(() => records.Count == 5).ConfigureAwait(false);
                sub.Dispose();

                Assert.Equal(5, records.Count);
            }
        }

        [Theory]
        [ClassData(typeof(TestConfigurations))]
        public async Task Can_execute_void_commands(MemstateSettings settings)
        {
            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;

            var builder = new EngineBuilder(settings);
            var engine = builder.Build<List<string>>();
            await engine.ExecuteAsync(new Reverse()).ConfigureAwait(false);
            engine.Dispose();
        }

        [Theory]
        [ClassData(typeof(TestConfigurations))]
        public async Task Smoke(MemstateSettings settings)
        {
            const int NumRecords = 100;

            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;

            var builder = new EngineBuilder(settings);
            var engine = builder.Build<List<string>>();

            foreach (var number in Enumerable.Range(1, NumRecords))
            {
                var command = new AddStringCommand(number.ToString());
                var count = await engine.ExecuteAsync(command).ConfigureAwait(false);
                Assert.Equal(number, count);
            }

            engine.Dispose();

            engine = builder.Build<List<string>>();
            var strings = await engine.ExecuteAsync(new GetStringsQuery()).ConfigureAwait(false);
            Assert.Equal(NumRecords, strings.Count);
            engine.Dispose();
        }

        private async Task WaitForConditionOrThrow(Func<bool> condition, TimeSpan? checkInterval = null, int numberOfTries = 25)
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