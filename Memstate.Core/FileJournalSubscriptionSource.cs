using System;
using System.Collections.Generic;

namespace Memstate.Core
{
    public class FileJournalSubscriptionSource : ICommandSubscriptionSource
    {
        private readonly Dictionary<Guid, Subscription> _subscriptions;
        private readonly FileJournalWriter _journalWriter;

        public FileJournalSubscriptionSource(FileJournalWriter journalWriter)
        {
            _journalWriter = journalWriter;
            _subscriptions = new Dictionary<Guid, Subscription>();
            _journalWriter.RecordsWritten += OnRecordsWritten;
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

        public ICommandSubscription Subscribe(long @from, Action<JournalRecord> handler)
        {
            var sub = new Subscription(handler, @from, OnDisposed);
            lock (_subscriptions)
            {
                _subscriptions.Add(sub.Id, sub);
            }
            return sub;
        }

        private void OnDisposed(Subscription subscription)
        {
            lock (_subscriptions)
            {
                _subscriptions.Remove(subscription.Id);
            }
        }

        public void Dispose()
        {
            _journalWriter.RecordsWritten -= OnRecordsWritten;
        }
    }
}