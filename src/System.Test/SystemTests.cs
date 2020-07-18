using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memstate;
using Memstate.Configuration;
using NUnit.Framework;

namespace System.Test
{
    [TestFixtureSource(typeof(TestConfigurations),  nameof(TestConfigurations.All))]
    public class SystemTests
    {

        private readonly Config _config;

        public SystemTests(Config config)
        {
            _config = config;
        }

        [Test]
        public async Task CanWriteOne()
        {
            var provider = _config.GetStorageProvider();
            var writer = provider.CreateJournalWriter();

            await writer.Write(new AddStringCommand("hello"));
            await writer.DisposeAsync();

            var reader = provider.CreateJournalReader();
            var records = reader.ReadRecords().ToArray();
            await reader.DisposeAsync();
            Assert.AreEqual(1, records.Length);
        }

        [Test]
        public async Task WriteAndSubscribeCommands()
        {
            var provider = _config.GetStorageProvider();

            var journalWriter = provider.CreateJournalWriter();

            for (var i = 0; i < 10000; i++)
            {
                await journalWriter.Write(new AddStringCommand(i.ToString()));
            }

            await journalWriter.DisposeAsync();
            var journalReader = provider.CreateJournalReader();
            var records = new List<JournalRecord>(10000);
            var task = journalReader.Subscribe(0, 9999, records.Add, CancellationToken.None);
            await task;
            await journalReader.DisposeAsync();
            Assert.AreEqual(10000, records.Count);
        }
        
        [Test]
        public async Task WriteAndReadCommands()
        {
            var provider = _config.GetStorageProvider();

            var journalWriter = provider.CreateJournalWriter();

            for (var i = 0; i < 10000; i++)
            {
                await journalWriter.Write(new AddStringCommand(i.ToString()));
            }

            await journalWriter.DisposeAsync();
            var journalReader = provider.CreateJournalReader();
            var records = journalReader.ReadRecords().ToArray();
            await journalReader.DisposeAsync();
            Assert.AreEqual(10000, records.Length);
        }

        [Test]
        public async Task SubscriptionDeliversPreExistingCommands()
        {
            var provider = _config.GetStorageProvider();
            const int numRecords = 50;
            var journalWriter = provider.CreateJournalWriter();
            for (var i = 0; i < numRecords; i++)
            {
                await journalWriter.Write(new AddStringCommand(i.ToString()));
            }

            await journalWriter.DisposeAsync();

            var records = new List<JournalRecord>();

            var reader = provider.CreateJournalReader();
            var token = new CancellationToken();
            {
                await reader.Subscribe(0, numRecords - 1, r =>
                {
                    records.Add(r);
                    Console.WriteLine("record received # " + r.RecordNumber);
                }, token);
                Assert.AreEqual(Enumerable.Range(0, numRecords), records.Select(r => (int)r.RecordNumber));
            }
        }

        [Test]
        public async Task SubscriptionDeliversFutureCommands()
        {
            const int numRecords = 5;

            var provider = _config.GetStorageProvider();
            var records = new List<JournalRecord>();
            var writer = provider.CreateJournalWriter();

            var reader = provider.CreateJournalReader();
            var subTask = reader.Subscribe(0, 4, records.Add, CancellationToken.None);

            for (var i = 0; i < numRecords; i++)
            {
                await writer.Write(new AddStringCommand(i.ToString()));
            }

            await writer.DisposeAsync();
            await subTask;

            Assert.AreEqual(numRecords, records.Count);
        }

        [Test]
        public async Task Can_execute_void_commands()
        {
            var engine = await Engine.Start<List<string>>(_config);
            await engine.Execute(new Reverse());
            await engine.DisposeAsync();
        }

        [Test]
        public async Task Smoke()
        {
            const int numRecords = 100;

            var engine = await Engine.Start<List<string>>(_config);

            foreach (var number in Enumerable.Range(1, numRecords))
            {
                var command = new AddStringCommand(number.ToString());
                var count = await engine.Execute(command);
                Assert.AreEqual(number, count);
            }

            await engine.DisposeAsync();

            engine = await Engine.Start<List<string>>(_config);
            var strings = await engine.Execute(new GetStringsQuery());
            Assert.AreEqual(numRecords, strings.Count);
            await engine.DisposeAsync();
        }
    }
}