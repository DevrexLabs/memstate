using System.Collections.Generic;
using System.Threading.Tasks;
using Streamstone;

namespace Memstate.Azure
{
    public class TableStorageJournalReader : JournalReader
    {
        private readonly Partition _partition;
        private readonly ISerializer _serializer;

        public TableStorageJournalReader(ISerializer serializer, Partition partition)
        {
            Ensure.NotNull(partition, nameof(partition));

            _partition = partition;
            _serializer = serializer;
        }

        public override IEnumerable<JournalRecord> ReadRecords(long fromRecord)
        {
            //StreamStone starts numbering from 1
            var version = (int)fromRecord + 1;
            
            while (true)
            {
                var slice = Stream.ReadAsync(_partition, version).NotOnCapturedContext().GetAwaiter().GetResult();
                foreach (var properties in slice.Events)
                {
                    yield return FromProperties(fromRecord++, properties);
                }
                
                if (slice.IsEndOfStream) break;
                version = slice.Stream.Version +1;
            }
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        private JournalRecord FromProperties(long recordNumber, EventProperties props)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var written = props["Written"].DateTimeOffsetValue.Value;
            var command = (Command) _serializer.FromString(props["Command"].StringValue);
            return new JournalRecord(recordNumber, written, command);
        }
    }
}