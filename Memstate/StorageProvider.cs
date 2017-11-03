namespace Memstate
{
    using System;

    public abstract class StorageProvider : IDisposable
    {
        public virtual void Initialize()
        {
        }

        public abstract IJournalReader CreateJournalReader();

        public abstract IJournalWriter CreateJournalWriter(long nextRecordNumber);

        public abstract IJournalSubscriptionSource CreateJournalSubscriptionSource();

        public abstract void Dispose();
    }
}