namespace Memstate
{
    using System;

    public abstract class StorageProvider : IDisposable
    {
        protected readonly Settings Config;

        protected StorageProvider(Settings config)
        {
            Config = config;
        }

        public abstract IJournalReader CreateJournalReader();

        public abstract IJournalWriter CreateJournalWriter(long nextRecordNumber);

        public abstract IJournalSubscriptionSource CreateJournalSubscriptionSource();

        public abstract void Dispose();
    }
}