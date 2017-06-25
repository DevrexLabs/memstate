using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Memstate.Core
{
    public class FileCommandStore : IHandle<Command>, ICommandSubscriptionSource
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
        private readonly Batcher<Command> _commandBatcher;
        private long _nextRecord;
        private readonly FileStream _journalStream;
        private readonly ISerializer _serializer;

        public FileCommandStore(String fileName, ISerializer serializer, long nextRecord = 1)
        {
            _journalStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _commandBatcher = new Batcher<Command>(OnCommandBatch, 100);
            _subscriptions = new Dictionary<Guid, Subscription>();
            _nextRecord = nextRecord;
            _serializer = serializer;
        }

        public void Dispose()
        {
            _commandBatcher.Dispose();
        }

        private JournalRecord ToJournalRecord(Command command)
        {
            return new JournalRecord(_nextRecord++, DateTime.Now, command);
        }

        private void OnCommandBatch(IEnumerable<Command> commands)
        {
            var memoryStream = new MemoryStream();
            var records = commands.Select(ToJournalRecord).ToArray();
            _serializer.Serialize(memoryStream, records);
            lock (_journalStream)
            {
                _journalStream.Position = _journalStream.Length + 1;
                memoryStream.CopyTo(_journalStream);
            }
            foreach (var record in records)
            {
                foreach (var sub in _subscriptions.Values)
                {
                    sub.Handle(record);
                }
            }
        }

        public void Handle(Command command)
        {
            _commandBatcher.Append(command);
        }

        private void RemoveSubscription(Subscription subscription)
        {
            lock (_subscriptions)
            {
                _subscriptions.Remove(subscription.Id);
            }
        }

        private IEnumerable<JournalRecord> GetRecords()
        {
            lock (_journalStream)
            {
                _journalStream.Position = 0;
                while (_journalStream.Position < _journalStream.Length)
                {
                    var records = (JournalRecord[])_serializer.Deserialize(_journalStream);
                    foreach (var record in records)
                    {
                        yield return record;
                    }
                }
            }
        }

        public ICommandSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            var subscription = new Subscription(handler, from, RemoveSubscription);
            lock (_subscriptions) _subscriptions.Add(subscription.Id, subscription);
            foreach (var record in GetRecords().SkipWhile(record => record.RecordNumber < from))
            {
                subscription.Handle(record);
            }
            return subscription;
        }
    }
}