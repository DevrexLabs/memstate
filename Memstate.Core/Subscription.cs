using System;

namespace Memstate.Core
{
    public class Subscription : IHandle<JournalRecord>, ICommandSubscription
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
}