using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memstate;
using Memstate.Configuration;
using NUnit.Framework;

namespace System.Test
{
    [TestFixture]
    public class SystemTests
    {
        public static IEnumerable<Config> Configurations()
        {
            return new TestConfigurations().GetConfigurations();
        }

        [TestCaseSource(nameof(Configurations))]
        public async Task CanWriteOne(Config config)
        {
            var provider = config.GetStorageProvider();
            await provider.Provision();
            var writer = provider.CreateJournalWriter();

            await writer.Write(new AddStringCommand("hello"));
            await writer.DisposeAsync().ConfigureAwait(false);

            var reader = provider.CreateJournalReader();
            var records = reader.ReadRecords().ToArray();
            await reader.DisposeAsync().ConfigureAwait(false);
            Assert.AreEqual(1, records.Length);
        }

        [TestCaseSource(nameof(Configurations))]
        public async Task WriteAndReadCommands(Config config)
        {
            var provider = config.GetStorageProvider();
            await provider.Provision();

            var journalWriter = provider.CreateJournalWriter();

            for (var i = 0; i < 10000; i++)
            {
                await journalWriter.Write(new AddStringCommand(i.ToString()));
            }

            await journalWriter.DisposeAsync().ConfigureAwait(false);
            var journalReader = provider.CreateJournalReader();
            var records = journalReader.ReadRecords().ToArray();
            await journalReader.DisposeAsync().ConfigureAwait(false);
            Assert.AreEqual(10000, records.Length);
        }

        [TestCaseSource(nameof(Configurations))]
        public async Task SubscriptionDeliversPreExistingCommands(Config config)
        {
            Console.WriteLine(config);

            var provider = config.GetStorageProvider();
            const int numRecords = 50;
            var journalWriter = provider.CreateJournalWriter();
            for (var i = 0; i < numRecords; i++)
            {
                await journalWriter.Write(new AddStringCommand(i.ToString()));
            }

            await journalWriter.DisposeAsync().NotOnCapturedContext();

            var records = new List<JournalRecord>();

            var reader = provider.CreateJournalReader();
            var token = new CancellationToken();
            {
                await reader.Subscribe(0, records.Count - 1, r =>
                {
                    records.Add(r);
                    Console.WriteLine("record received # " + r.RecordNumber);
                }, token);
                Assert.AreEqual(Enumerable.Range(0, numRecords), records.Select(r => (int)r.RecordNumber));
            }
        }

        [TestCaseSource(nameof(Configurations))]
        public async Task SubscriptionDeliversFutureCommands(Config config)
        {
            const int numRecords = 5;
            Console.WriteLine(config);

            var provider = config.GetStorageProvider();
            var records = new List<JournalRecord>();
            var writer = provider.CreateJournalWriter();

            var reader = provider.CreateJournalReader();
            var subTask = reader.Subscribe(0, 4, records.Add, CancellationToken.None);

            for (var i = 0; i < numRecords; i++)
            {
                await writer.Write(new AddStringCommand(i.ToString()));
            }

            await writer.DisposeAsync().NotOnCapturedContext();
            await subTask;

            Assert.AreEqual(numRecords, records.Count);
        }

        [TestCaseSource(nameof(Configurations))]
        public async Task Can_execute_void_commands(Config config)
        {
            Console.WriteLine(config);
            var engine = await Engine.Start<List<string>>(config);
            await engine.Execute(new Reverse());
            await engine.DisposeAsync();
        }

        [TestCaseSource(nameof(Configurations))]
        public async Task Smoke(Config config)
        {
            const int NumRecords = 100;
            
            Console.WriteLine(config);

            var engine = await Engine.Start<List<string>>(config).ConfigureAwait(false);

            foreach (var number in Enumerable.Range(1, NumRecords))
            {
                var command = new AddStringCommand(number.ToString());
                var count = await engine.Execute(command);
                Assert.AreEqual(number, count);
            }

            await engine.DisposeAsync();

            engine = await Engine.Start<List<string>>(config).ConfigureAwait(false);
            var strings = await engine.Execute(new GetStringsQuery()).ConfigureAwait(false);
            Assert.AreEqual(NumRecords, strings.Count);
            await engine.DisposeAsync();
        }
    }
}