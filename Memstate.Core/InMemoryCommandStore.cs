using System;
using System.Collections.Generic;

namespace Memstate.Core
{
    public class InMemoryCommandStore : IHandle<Command>, ICommandSubscriptionSource
    {
        class Subscription : IHandle<JournalRecord>, ICommandSubscription
        {
            public readonly Guid Id = Guid.NewGuid();
            private readonly Action<JournalRecord> _callback;
            private long _nextRecord;
            private readonly Action<Subscription> _onDisposed;

            public Subscription(Action<JournalRecord> callback, long nextRecord, Action<Subscription> onDisposed)
            {
                _nextRecord = nextRecord;
                _callback = callback;
                _onDisposed = onDisposed;
            }

            public void Dispose()
            {
                _onDisposed.Invoke(this);
            }

            public bool Ready()
            {
                return true;
            }

            public void Handle(JournalRecord record)
            {
                if (record.RecordNumber != _nextRecord) throw new InvalidOperationException("expected version " + _nextRecord + ", got " + record.RecordNumber);
                _nextRecord++;
                _callback.Invoke(record);
            }
        }

        private readonly Dictionary<Guid, Subscription> _subscriptions;
        private readonly Batcher<Command> _batchingLogger;
        private long _nextRecord;
        private readonly List<JournalRecord> _journal = new List<JournalRecord>();
      

        public InMemoryCommandStore(long nextRecord = 1)
        {
            _batchingLogger = new Batcher<Command>(OnCommandBatch, 100);
            _subscriptions = new Dictionary<Guid, Subscription>();
            _nextRecord = nextRecord;
        }

        public void Dispose()
        {
            _batchingLogger.Dispose();
        }

        private void OnCommandBatch(IEnumerable<Command> commands)
        {
            lock (_journal)
            {
                foreach (var command in commands)
                {
                    var record = new JournalRecord(_nextRecord++, DateTime.Now, command);
                    _journal.Add(record);
                    foreach (var sub in _subscriptions.Values)
                    {
                        sub.Handle(record);
                    }
                }
            }
        }

        public void Handle(Command command)
        {
            _batchingLogger.Append(command);
        }

        private void RemoveSubscription(Subscription subscription)
        {
            lock (_journal)
            {
                _subscriptions.Remove(subscription.Id);
            }
        }

        public ICommandSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            var subscription = new Subscription(handler, from, RemoveSubscription);
            lock (_journal)
            {
                _subscriptions.Add(subscription.Id, subscription);
                for (int i = (int)from - 1; i < _journal.Count; i++)
                {
                    subscription.Handle(_journal[i]);
                }
            }
            return subscription;
        }
    }
}