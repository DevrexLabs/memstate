using System.ComponentModel;
using System.Text;
using Memstate.Configuration;
using Npgsql;
using SqlStreamStore;

namespace Memstate.SqlStreamStore
{
    public static class ConfigExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="streamStore"></param>
        /// <param name="useSubscriptionBasedReader">
        /// There are two reader implementations. The subscriptionBasedReader
        /// will in some cases have a better performance, the default is false</param>
        /// <param name="maxRecordsPerRead">The number of  to fetch</param>
        /// <returns></returns>
        public static Config UseSqlStreamStore(this Config config, 
            IStreamStore streamStore,
            bool useSubscriptionBasedReader = false,
            int maxRecordsPerRead = 100)
        {
            var settings = config.GetSettings<SqlStreamStoreSettings>();
            settings.UseSubscriptionBasedReader = useSubscriptionBasedReader;
            settings.MaxRecordsPerRead = maxRecordsPerRead;
  
            config.StorageProviderName = StorageProviders.SqlStreamStore;
            config.Container.Register(streamStore);
            return config;
        }
    }
}