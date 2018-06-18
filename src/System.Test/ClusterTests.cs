using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memstate;
using NUnit.Framework;

namespace System.Test
{
    [TestFixture]
    public class ClusterTests
    {
        private string _randomStreamName;

        [SetUp]
        public void Setup()
        {
            _randomStreamName 
                = "memstate" + Guid.NewGuid().ToString("N").Substring(0, 10);
        }

        // One writer, multiple readers
        [TestCaseSource(nameof(Configurations))]
        public async Task CanWriteOneAndReadFromMany(MemstateSettings settings)
        {
            const int records = 100;

            Configure(settings);

            var writer = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);

            var readers = new Engine<List<string>>[3];

            readers[0] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[1] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[2] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);

            foreach (var number in Enumerable.Range(1, records))
            {
                var command = new AddStringCommand($"{number}");
                var count = await writer.Execute(command).ConfigureAwait(false);
                Assert.AreEqual(number, count);
            }

            await writer.DisposeAsync().ConfigureAwait(false);

            foreach (var reader in readers)
            {
                await reader.EnsureVersion(writer.LastRecordNumber);
                
                var strings = await reader.Execute(new GetStringsQuery()).ConfigureAwait(false);

                Assert.AreEqual(records, strings.Count);

                await reader.DisposeAsync().ConfigureAwait(false);
            }
        }

        // Multiple writers, one reader
        [TestCaseSource(nameof(Configurations))]
        public async Task CanWriteManyAndReadFromOne(MemstateSettings settings)
        {
            const int records = 100;

            Configure(settings);


            var reader = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);

            var writers = new Engine<List<string>>[3];

            writers[0] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            writers[1] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            writers[2] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);

            var totalCount = 0;

            foreach (var writer in writers)
            {
                foreach (var number in Enumerable.Range(1, records))
                {
                    var command = new AddStringCommand($"{number}");
                    var count = await writer.Execute(command).ConfigureAwait(false);
                    Assert.AreEqual(++totalCount, count);
                }

                await writer.DisposeAsync().ConfigureAwait(false);
            }

            var strings = await reader.Execute(new GetStringsQuery()).ConfigureAwait(false);

            Assert.AreEqual(records * writers.Length, strings.Count);

            await reader.DisposeAsync().ConfigureAwait(false);
        }

        // Multiple writers, multiple readers
        [TestCaseSource(nameof(Configurations))]
        public async Task CanWriteManyAndReadFromMany(MemstateSettings settings)
        {
            const int records = 100;

            Configure(settings);


            var readers = new Engine<List<string>>[3];

            readers[0] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[1] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[2] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);

            var writers = new Engine<List<string>>[3];

            writers[0] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            writers[1] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            writers[2] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);

            var totalCount = 0;

            foreach (var writer in writers)
            {
                foreach (var number in Enumerable.Range(1, records))
                {
                    var command = new AddStringCommand($"{number}");
                    var count = await writer.Execute(command).ConfigureAwait(false);
                    Assert.AreEqual(++totalCount, count);
                }

                await writer.DisposeAsync().ConfigureAwait(false);
            }

            foreach (var reader in readers)
            {
                //await reader.EnsureVersion(totalCount);
                var strings = await reader.Execute(new GetStringsQuery()).ConfigureAwait(false);

                Assert.AreEqual(totalCount, strings.Count);

                await reader.DisposeAsync().ConfigureAwait(false);
            }
        }

        // Multiple writers, multiple readers, in parallel
        [TestCaseSource(nameof(Configurations))]
        public async Task CanWriteManyAndReadFromManyInParallel(MemstateSettings settings)
        {
            const int Records = 100;

            Configure(settings);

            Console.WriteLine("Creating readers");
            var readers = new Engine<List<string>>[3];

            readers[0] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[1] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[2] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            Console.WriteLine("Readers created");

            Console.WriteLine("Creating writers");
            var writers = new Engine<List<string>>[3];

            writers[0] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            writers[1] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            writers[2] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            Console.WriteLine("Writers created");

            Console.WriteLine("Creating write tasks");
            var tasks = writers.Select(
                (writer, index) => Task.Run(
                    async () =>
                        {
                            Console.WriteLine($"Writing commands on writer-{index + 1}");
                            foreach (var number in Enumerable.Range(1, Records))
                            {
                                var command = new AddStringCommand($"{index}.{number}");
                                await writer.Execute(command).ConfigureAwait(false);
                            }

                            Console.WriteLine($"Done writing commands on writer-{index + 1}");
                        })).ToArray();

            Console.WriteLine("Waiting on write tasks");
            await Task.WhenAll(tasks).ConfigureAwait(false);
            var recordsWritten = Records * writers.Length;
            Console.WriteLine("Done waiting on write tasks");

            Console.WriteLine("Reading from all engines");
            var engines = readers.Concat(writers);

            foreach (var engine in engines)
            {
                Console.WriteLine("Counting strings");
                await engine.EnsureVersion(recordsWritten - 1).ConfigureAwait(false);
                var strings = await engine.Execute(new GetStringsQuery()).ConfigureAwait(false);

                Console.WriteLine($"Count: {strings.Count}");

                Assert.AreEqual(recordsWritten, strings.Count);

                Console.WriteLine("Disposing reader");
                await engine.DisposeAsync().ConfigureAwait(false);
                Console.WriteLine("Disposed reader");
            }
            Console.WriteLine("Done reading from all engines");
        }

        private void Configure(MemstateSettings settings)
        {
            settings.StreamName = _randomStreamName;
            Console.WriteLine("C: " + settings);
        }

        public static IEnumerable<MemstateSettings> Configurations()
        {
            return new TestConfigurations.Cluster().GetConfigurations();
        }
    }
}