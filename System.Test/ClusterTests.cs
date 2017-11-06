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
    }
}