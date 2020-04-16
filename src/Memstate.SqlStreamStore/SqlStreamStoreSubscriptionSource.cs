using System;
using System.Threading;
using System.Threading.Tasks;
using SqlStreamStore;
using SqlStreamStore.Logging;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreSubscriptionSource
        : IJournalSubscriptionSource
    {
        private class SqlStreamStoreSubscription : IJournalSubscription
        {
            private readonly Func<bool> _readyDelegate;
            private readonly IStreamSubscription _subscription;

            public SqlStreamStoreSubscription(IStreamSubscription sub, Func<bool> readyDelegate)
            {
                _readyDelegate = readyDelegate;
                _subscription = sub;
            }
            public void Dispose()
                => _subscription.Dispose();

            public bool Ready()
                => _readyDelegate.Invoke();
        }

        private readonly IStreamStore _streamStore;
        private readonly StreamId _streamId;
        private readonly ISerializer _serializer;

        private readonly ILog _log;

        public SqlStreamStoreSubscriptionSource(IStreamStore streamStore, StreamId streamId, ISerializer serializer)
        {
            _serializer = serializer;
            _streamId = streamId;
            _streamStore = streamStore;
            _log = LogProvider.For<SqlStreamStoreSubscriptionSource>();
        }

        public IJournalSubscription Subscribe(long @from, Action<JournalRecord> handler)
        {
            async Task MessageReceived(IStreamSubscription subscription, StreamMessage message,
                CancellationToken cancellationToken)
            {
                var command = (Command)_serializer.FromString(await message.GetJsonData());
                var journalRecord = new JournalRecord(message.StreamVersion, message.CreatedUtc, command);
                handler.Invoke(journalRecord);
            }

            // pass null to subscribe from the beginning
            //or the version of the previous record
            int? version = null;
            if (from > 0) version = (int)from - 1;

            var caughtUp = false;

            var sub = _streamStore.SubscribeToStream(
                _streamId,
                version,
                MessageReceived,
                SubscriptionDropped,
                hasCaughtUp => caughtUp = hasCaughtUp);

            sub.MaxCountPerRead = 100;

            return new SqlStreamStoreSubscription(sub, () => caughtUp);
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