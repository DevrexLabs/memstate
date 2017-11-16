using System;
using System.Collections.Generic;

namespace Memstate
{
    public class FileJournalSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly Dictionary<Guid, JournalSubscription> _subscriptions;
        private readonly FileJournalWriter _journalWriter;

        public FileJournalSubscriptionSource(FileJournalWriter journalWriter)
        {
            _journalWriter = journalWriter;
            _subscriptions = new Dictionary<Guid, JournalSubscription>();
            _journalWriter.RecordsWritten += OnRecordsWritten;
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            if (from != _journalWriter.NextRecord)
            {
                throw new NotSupportedException("CatchupSubscriptions are not supported by this FileStorageProvider");
            }

            var sub = new JournalSubscription(handler, from, OnDisposed);
            lock (_subscriptions)
            {
                _subscriptions.Add(sub.Id, sub);
            }

            return sub;
        }

        public void Dispose()
        {
            _journalWriter.RecordsWritten -= OnRecordsWritten;
        }

        private void OnRecordsWritten(JournalRecord[] records)
        {
            lock (_subscriptions)
            {
                foreach (var subscription in _subscriptions.Values)
                {
                    foreach (var record in records)
                    {
                        subscription.Handle(record);
                    }
                }
            }
        }

        private void OnDisposed(JournalSubscription subscription)
        {
            lock (_subscriptions)
            {
                _subscriptions.Remove(subscription.Id);
            }
        }
    }
}