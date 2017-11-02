namespace Memstate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class InMemoryStorageProvider : StorageProvider, IJournalWriter, IJournalReader, IJournalSubscriptionSource
    {
        private readonly Dictionary<Guid, JournalSubscription> _subscriptions;
        private readonly Batcher<Command> _batchingLogger;
        private readonly List<JournalRecord> _journal = new List<JournalRecord>();

        private long _nextRecord;

        public InMemoryStorageProvider(Settings settings) : this(settings, 0)
        {
        }

        public InMemoryStorageProvider(Settings settings, long nextRecord = 0) 
            : base(settings)
        {
            _batchingLogger = new Batcher<Command>(settings, 100);
            _batchingLogger.OnBatch += OnCommandBatch;
            _subscriptions = new Dictionary<Guid, JournalSubscription>();
            _nextRecord = nextRecord;
        }

        public override IJournalReader CreateJournalReader()
        {
            return this;
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            _nextRecord = nextRecordNumber;
            return this;
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return this;
        }

        public override void Dispose()
        {
            _batchingLogger.Dispose();
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            return _journal.Skip((int)fromRecord);
        }

        public void Send(Command command)
        {
            _batchingLogger.Add(command);
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            var subscription = new JournalSubscription(handler, from, RemoveSubscription);
            lock (_journal)
            {
                _subscriptions.Add(subscription.Id, subscription);
                for (var i = (int)from; i < _journal.Count; i++)
                {
                    subscription.Handle(_journal[i]);
                }
            }

            return subscription;
        }

        private void RemoveSubscription(JournalSubscription subscription)
        {
            lock (_journal)
            {
                _subscriptions.Remove(subscription.Id);
            }
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
    }
}