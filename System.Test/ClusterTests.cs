using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memstate;
using Xunit;
using Xunit.Abstractions;

namespace System.Test
{
    public class ClusterTests
    {
        private readonly ITestOutputHelper _log;
        private readonly string _randomStreamName;

        public ClusterTests(ITestOutputHelper log)
        {
            _log = log;
            _randomStreamName = "memstate" + Guid.NewGuid().ToString("N").Substring(0, 10);
        }

        // One writer, multiple readers
        [Theory]
        [ClassData(typeof(TestConfigurations.Cluster))]
        public async Task CanWriteOneAndReadFromMany(MemstateSettings settings)
        {
            const int records = 100;

            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;

            var writer = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);

            var readers = new Engine<List<string>>[3];

            readers[0] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[1] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[2] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);

            foreach (var number in Enumerable.Range(1, records))
            {
                var command = new AddStringCommand($"{number}");
                var count = await writer.ExecuteAsync(command).ConfigureAwait(false);
                Assert.Equal(number, count);
            }

            await writer.DisposeAsync().ConfigureAwait(false);

            foreach (var reader in readers)
            {
                var strings = await reader.ExecuteAsync(new GetStringsQuery()).ConfigureAwait(false);

                Assert.Equal(records, strings.Count);

                await reader.DisposeAsync().ConfigureAwait(false);
            }
        }

        // Multiple writers, one reader
        [Theory]
        [ClassData(typeof(TestConfigurations.Cluster))]
        public async Task CanWriteManyAndReadFromOne(MemstateSettings settings)
        {
            const int records = 100;

            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;

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
                    var count = await writer.ExecuteAsync(command).ConfigureAwait(false);
                    Assert.Equal(++totalCount, count);
                }

                await writer.DisposeAsync().ConfigureAwait(false);
            }

            var strings = await reader.ExecuteAsync(new GetStringsQuery()).ConfigureAwait(false);

            Assert.Equal(records * writers.Length, strings.Count);

            await reader.DisposeAsync().ConfigureAwait(false);
        }

        // Multiple writers, multiple readers
        [Theory]
        [ClassData(typeof(TestConfigurations.Cluster))]
        public async Task CanWriteManyAndReadFromMany(MemstateSettings settings)
        {
            const int records = 100;

            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;

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
                    var count = await writer.ExecuteAsync(command).ConfigureAwait(false);
                    Assert.Equal(++totalCount, count);
                }

                await writer.DisposeAsync().ConfigureAwait(false);
            }

            foreach (var reader in readers)
            {
                var strings = await reader.ExecuteAsync(new GetStringsQuery()).ConfigureAwait(false);

                Assert.Equal(records * writers.Length, strings.Count);

                await reader.DisposeAsync().ConfigureAwait(false);
            }
        }

        // Multiple writers, multiple readers, in parallel
        [Theory]
        [ClassData(typeof(TestConfigurations.Cluster))]
        public async Task CanWriteManyAndReadFromManyInParallel(MemstateSettings settings)
        {
            const int Records = 100;

            settings.LoggerFactory.AddProvider(new TestOutputLoggingProvider(_log));
            settings.StreamName = _randomStreamName;

            _log.WriteLine("Creating readers");
            var readers = new Engine<List<string>>[3];

            readers[0] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[1] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            readers[2] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            _log.WriteLine("Readers created");

            _log.WriteLine("Creating writers");
            var writers = new Engine<List<string>>[3];

            writers[0] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            writers[1] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            writers[2] = await Engine.StartAsync<List<string>>(settings).ConfigureAwait(false);
            _log.WriteLine("Writers created");

            _log.WriteLine("Creating write tasks");
            var tasks = writers.Select(
                    (writer, index) => Task.Run(
                        async () =>
                        {
                            _log.WriteLine($"Writing commands on writer-{index+1}");
                            foreach (var number in Enumerable.Range(1, Records))
                            {
                                var command = new AddStringCommand($"{index}.{number}");
                                await writer.ExecuteAsync(command).ConfigureAwait(false);
                            }

                            await writer.DisposeAsync().ConfigureAwait(false);
                            _log.WriteLine($"Done writing commands on writer-{index+1}");
                        }))
                .ToArray();
            
            _log.WriteLine("Waiting on write tasks");
            await Task.WhenAll(tasks).ConfigureAwait(false);
            var recordsWritten = Records * writers.Length;
            _log.WriteLine("Done waiting on write tasks");

            _log.WriteLine("Reading from all engines");
            var engines = readers.Concat(writers);

            foreach (var engine in engines)
            {
                _log.WriteLine("Counting strings");
                await engine.EnsureAsync(recordsWritten - 1).ConfigureAwait(false);
                var strings = await engine.ExecuteAsync(new GetStringsQuery()).ConfigureAwait(false);
                
                _log.WriteLine($"Count: {strings.Count}");

                Assert.Equal(recordsWritten, strings.Count);

                _log.WriteLine("Disposing reader");
                await engine.DisposeAsync().ConfigureAwait(false);
                _log.WriteLine("Disposed reader");
            }
            _log.WriteLine("Done reading from all engines");
        }
    }
}