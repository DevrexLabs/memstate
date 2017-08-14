using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memstate.Core
{
    public class InMemoryCommandStore : IJournalWriter, IJournalSubscriptionSource
    {
        private readonly Dictionary<Guid, JournalSubscription> _subscriptions;
        private readonly Batcher<Command> _batchingLogger;
        private long _nextRecord;
        private readonly List<JournalRecord> _journal = new List<JournalRecord>();
      

        public InMemoryCommandStore(Config config, long nextRecord = 0)
        {
            _batchingLogger = new Batcher<Command>(config, 100);
            _batchingLogger.OnBatch += OnCommandBatch;

            _subscriptions = new Dictionary<Guid, JournalSubscription>();
            _nextRecord = nextRecord;
        }

        public void Dispose()
        {
            _batchingLogger.Dispose();
        }

        private Task OnCommandBatch(IEnumerable<Command> commands)
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
            return Task.CompletedTask;
        }

        public void Send(Command command)
        {
            _batchingLogger.Add(command);
        }

        private void RemoveSubscription(JournalSubscription subscription)
        {
            lock (_journal)
            {
                _subscriptions.Remove(subscription.Id);
            }
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            var subscription = new JournalSubscription(handler, from, RemoveSubscription);
            lock (_journal)
            {
                _subscriptions.Add(subscription.Id, subscription);
                for (int i = (int)from; i < _journal.Count; i++)
                {
                    subscription.Handle(_journal[i]);
                }
            }
            return subscription;
        }
    }
}