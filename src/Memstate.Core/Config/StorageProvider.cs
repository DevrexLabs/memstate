namespace Memstate
{
    public abstract class StorageProvider
    {
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// True if a subscription can start from an arbitrary record in the past
        /// </summary>
        public virtual bool SupportsCatchupSubscriptions() => true;

        public abstract IJournalReader CreateJournalReader();

        public abstract IJournalWriter CreateJournalWriter(long nextRecordNumber);

        public abstract IJournalSubscriptionSource CreateJournalSubscriptionSource();
    }
}