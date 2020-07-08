using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Logging;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreSubscriptionJournalReader : JournalReader
    {
        private readonly IStreamStore _streamStore;
        private readonly StreamId _streamId;
        private readonly ISerializer _serializer;

        private readonly ILog _log;

        public SqlStreamStoreSubscriptionJournalReader(IStreamStore streamStore, StreamId streamId, ISerializer serializer)
        {
            _serializer = serializer;
            _streamId = streamId;
            _streamStore = streamStore;
            _log = LogProvider.GetLogger(nameof(SqlStreamStoreSubscriptionJournalReader));
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        public override IEnumerable<JournalRecord> ReadRecords(long fromRecord)
        {
            using (var queue = new BlockingCollection<JournalRecord>())
            {
                async Task MessageReceived(IStreamSubscription subscription, StreamMessage message,
                    CancellationToken cancellationToken)
                {
                    var json = await message.GetJsonData(cancellationToken);
                    var command = (Command)_serializer.FromString(json);
                    var journalRecord = new JournalRecord(message.StreamVersion, message.CreatedUtc, command);
                    queue.Add(journalRecord, cancellationToken);
                }

                // pass null to subscribe from the beginning
                //or the version of the previous record
                int? version = null;
                if (fromRecord > 0) version = (int)fromRecord - 1;

                var caughtUp = false;

                using (
                    var sub = _streamStore.SubscribeToStream(
                        _streamId,
                        version,
                        MessageReceived,
                        SubscriptionDropped,
                        hasCaughtUp => caughtUp = hasCaughtUp))
                {
                    sub.MaxCountPerRead = 100;

                    while (!caughtUp || queue.Any())
                    {
                        if (queue.TryTake(out var journalRecord))
                            yield return journalRecord;
                        else if (!caughtUp)
                            Thread.Sleep(100);
                    }
                }
            }
        }

        private void SubscriptionDropped(IStreamSubscription subscription, SubscriptionDroppedReason reason,
            Exception ex)
        {
            if (reason != SubscriptionDroppedReason.Disposed)
            {
                _log.FatalException("Subscription dropped", ex);
                Environment.Exit(1);
            }
        }
    }
}
