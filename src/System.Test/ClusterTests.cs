using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memstate;
using Memstate.Configuration;
using NUnit.Framework;

namespace System.Test
{
    [TestFixture]
    public class ClusterTests
    {
        // One writer, multiple readers
        [TestCaseSource(nameof(Configurations))]
        public async Task CanWriteOneAndReadFromMany(Config config)
        {
            const int records = 100;

            var writer = await Engine.Start<List<string>>(config).NotOnCapturedContext();

            var readers = new Engine<List<string>>[3];

            readers[0] = await Engine.Start<List<string>>(config).NotOnCapturedContext();
            readers[1] = await Engine.Start<List<string>>(config).NotOnCapturedContext();
            readers[2] = await Engine.Start<List<string>>(config).NotOnCapturedContext();

            foreach (var number in Enumerable.Range(1, records))
            {
                var command = new AddStringCommand($"{number}");
                var count = await writer.Execute(command).NotOnCapturedContext();
                Assert.AreEqual(number, count);
            }

            await writer.DisposeAsync().NotOnCapturedContext();

            foreach (var reader in readers)
            {
                await reader.EnsureVersion(writer.LastRecordNumber);
                
                var strings = await reader.Execute(new GetStringsQuery()).NotOnCapturedContext();

                Assert.AreEqual(records, strings.Count);

                await reader.DisposeAsync().NotOnCapturedContext();
            }
        }

        // Multiple writers, one reader
        [TestCaseSource(nameof(Configurations))]
        public async Task CanWriteManyAndReadFromOne(Config config)
        {
            const int records = 100;

            var reader = await Engine.Start<List<string>>(config);

            var writers = new Engine<List<string>>[3];

            writers[0] = await Engine.Start<List<string>>(config);
            writers[1] = await Engine.Start<List<string>>(config);
            writers[2] = await Engine.Start<List<string>>(config);

            var totalCount = 0;

            foreach (var writer in writers)
            {
                foreach (var number in Enumerable.Range(1, records))
                {
                    var command = new AddStringCommand($"{number}");
                    var count = await writer.Execute(command);
                    Assert.AreEqual(++totalCount, count);
                }

                await writer.DisposeAsync();
            }

            var strings = await reader.Execute(new GetStringsQuery());

            Assert.AreEqual(records * writers.Length, strings.Count);

            await reader.DisposeAsync();
        }

        // Multiple writers, multiple readers
        [TestCaseSource(nameof(Configurations))]
        public async Task CanWriteManyAndReadFromMany(Config config)
        {
            const int records = 100;
            
            var readers = new Engine<List<string>>[3];

            readers[0] = await Engine.Start<List<string>>(config);
            readers[1] = await Engine.Start<List<string>>(config);
            readers[2] = await Engine.Start<List<string>>(config);

            var writers = new Engine<List<string>>[3];

            writers[0] = await Engine.Start<List<string>>(config);
            writers[1] = await Engine.Start<List<string>>(config);
            writers[2] = await Engine.Start<List<string>>(config);

            var totalCount = 0;

            foreach (var writer in writers)
            {
                foreach (var number in Enumerable.Range(1, records))
                {
                    var command = new AddStringCommand($"{number}");
                    var count = await writer.Execute(command);
                    Assert.AreEqual(++totalCount, count);
                }

                await writer.DisposeAsync();
            }

            for(var i = 0; i < readers.Length; i++)
            {
                var reader = readers[i];
                Console.WriteLine("Reader index: " + i);

                //await reader.EnsureVersion(totalCount);
                var strings = await reader.Execute(new GetStringsQuery());

                Assert.AreEqual(totalCount, strings.Count);

                await reader.DisposeAsync();
            }
        }

        // Multiple writers, multiple readers, in parallel
        [TestCaseSource(nameof(Configurations))]
        public async Task CanWriteManyAndReadFromManyInParallel(Config config)
        {
            const int Records = 100;

            Console.WriteLine("Creating readers");
            var readers = new Engine<List<string>>[3];

            readers[0] = await Engine.Start<List<string>>(config);
            readers[1] = await Engine.Start<List<string>>(config);
            readers[2] = await Engine.Start<List<string>>(config);
            Console.WriteLine("Readers created");

            Console.WriteLine("Creating writers");
            var writers = new Engine<List<string>>[3];

            writers[0] = await Engine.Start<List<string>>(config);
            writers[1] = await Engine.Start<List<string>>(config);
            writers[2] = await Engine.Start<List<string>>(config);
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
                                await writer.Execute(command);
                            }

                            Console.WriteLine($"Done writing commands on writer-{index + 1}");
                        })).ToArray();

            Console.WriteLine("Waiting on write tasks");
            await Task.WhenAll(tasks);
            var recordsWritten = Records * writers.Length;
            Console.WriteLine("Done waiting on write tasks");

            Console.WriteLine("Reading from all engines");
            var engines = readers.Concat(writers);

            foreach (var engine in engines)
            {
                Console.WriteLine("Counting strings");
                await engine.EnsureVersion(recordsWritten - 1);
                var strings = await engine.Execute(new GetStringsQuery());

                Console.WriteLine($"Count: {strings.Count}");

                Assert.AreEqual(recordsWritten, strings.Count);

                Console.WriteLine("Disposing reader");
                await engine.DisposeAsync();
                Console.WriteLine("Disposed reader");
            }
            Console.WriteLine("Done reading from all engines");
        }

        public static IEnumerable<Config> Configurations()
        {
            return new TestConfigurations.Cluster().GetConfigurations();
        }
    }
}