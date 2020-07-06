using Fig;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreSettings : Settings
    {
        public SqlStreamStoreSettings() : base("Memstate.SqlStreamStore")
        {
        }

        public bool UseSubscriptionBasedReader { get; set; }
    
        public int MaxRecordsPerRead { get; set; }
    }
}