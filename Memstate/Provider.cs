namespace Memstate
{
    using System;

    public abstract class Provider : IDisposable
    {
        protected readonly Settings Config;

        protected Provider(Settings config)
        {
            Config = config;
        }

        public abstract IJournalReader CreateJournalReader();

        public abstract IJournalWriter CreateJournalWriter();

        public abstract IJournalSubscriptionSource CreateJournalSubscriptionSource();

        public abstract void Dispose();
    }
}