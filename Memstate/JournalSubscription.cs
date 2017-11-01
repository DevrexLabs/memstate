namespace Memstate
{
    using System;

    public class JournalSubscription : IJournalSubscription
    {
        public readonly Guid Id = Guid.NewGuid();
        private readonly Action<JournalRecord> _callback;
        private readonly Action<JournalSubscription> _onDisposed;
        private long _nextRecord;
        
        public JournalSubscription(Action<JournalRecord> callback, long nextRecord, Action<JournalSubscription> onDisposed)
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
            if (record.RecordNumber != _nextRecord)
            {
                throw new InvalidOperationException("expected version " + _nextRecord + ", got " + record.RecordNumber);
            }

            _nextRecord++;
            _callback.Invoke(record);
        }
    }
}