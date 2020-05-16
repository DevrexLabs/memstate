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
    public class SqlSteamStoreSubscriptionJournalReader : IJournalReader
    {
        private readonly IStreamStore _streamStore;
        private readonly StreamId _streamId;
        private readonly ISerializer _serializer;

        private readonly ILog _log;

        public SqlSteamStoreSubscriptionJournalReader(IStreamStore streamStore, StreamId streamId, ISerializer serializer)
        {
            _serializer = serializer;
            _streamId = streamId;
            _streamStore = streamStore;
            _log = LogProvider.For<SqlStreamStoreSubscriptionSource>();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            BlockingCollection<JournalRecord> queue = new BlockingCollection<JournalRecord>();

            async Task MessageReceived(IStreamSubscription subscription, StreamMessage message,
                CancellationToken cancellationToken)
            {
                if (message.StreamVersion < fromRecord)
                    return;
                var command = (Command)_serializer.FromString(await message.GetJsonData());
                var journalRecord = new JournalRecord(message.StreamVersion, message.CreatedUtc, command);
                queue.Add(journalRecord);
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

                JournalRecord journalRecord;
                while (!caughtUp || queue.Any())
                {

                    if (queue.TryTake(out journalRecord))
                        yield return journalRecord;
                    else if (!caughtUp)
                        Thread.Sleep(100);
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
