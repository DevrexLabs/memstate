namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreProvider: StorageProvider
    {
        public override IJournalReader CreateJournalReader()
        {
            return new SqlStreamSourceJournalReader();
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            return new SqlStreamStoreJournalWriter(nextRecordNumber);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new SqlStreamStoreSubscriptionSource();
        }
    }
}