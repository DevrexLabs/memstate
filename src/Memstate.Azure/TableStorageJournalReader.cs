using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Configuration;
using Microsoft.Azure.Cosmos.Table;
using Streamstone;

namespace Memstate.Azure
{
    public class TableStorageJournalReader : IJournalReader
    {
        private readonly Partition _partition;
        private readonly ISerializer _serializer;

        public TableStorageJournalReader(Partition partition)
        {
            Ensure.NotNull(partition, nameof(partition));

            _partition = partition;
            _serializer = Config.Current.CreateSerializer();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            //StreamStone starts numbering from 1
            var version = (int)fromRecord + 1;
            
            while (true)
            {
                var slice = Stream.ReadAsync(_partition, version).ConfigureAwait(false).GetAwaiter().GetResult();
                foreach (var properties in slice.Events)
                {
                    yield return FromProperties(fromRecord++, properties);
                }
                
                if (slice.IsEndOfStream) break;
                version = slice.Stream.Version + 1;
            }
        }

        private JournalRecord FromProperties(long recordNumber, EventProperties props)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var written = props["Written"].DateTimeOffsetValue.Value;
            var command = (Command) _serializer.FromString(props["Command"].StringValue);
            return new JournalRecord(recordNumber, written, command);
        }
    }
}