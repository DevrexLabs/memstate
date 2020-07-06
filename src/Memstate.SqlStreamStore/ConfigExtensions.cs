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
            IStreamStore streamStore = null,
            bool useSubscriptionBasedReader = false,
            int maxRecordsPerRead = 100)
        {
            //TODO: Fix this, we need a way to inject 
            var settings = config.GetSettings<SqlStreamStoreSettings>();
            settings.UseSubscriptionBasedReader = useSubscriptionBasedReader;
            settings.MaxRecordsPerRead = maxRecordsPerRead;
            
            //config.Data["SqlStreamStore.UseSubscriptionBasedReader"] = useSubscriptionBasedReader.ToString();
            //config.Data["SqlStreamStore.MaxRecordsPerRead"] = maxRecordsPerRead.ToString();
            
            config.StorageProviderName = StorageProviders.SqlStreamStore;
            config.Container.Register(streamStore);
            return config;
        }
    }
}