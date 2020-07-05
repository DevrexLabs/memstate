using Memstate.Configuration;
using Microsoft.Azure.Cosmos.Table;

namespace Memstate.Azure
{
    public static class ConfigExtensions
    {
        public static Config UseAzureTableStorage(this Config config, CloudTable cloudTable)
        {
            //config.Data["StreamStone.UseSubscriptionBasedReader"] = useSubscriptionBasedReader.ToString();
            //config.Data["StreamStone.MaxRecordsPerRead"] = maxRecordsPerRead.ToString();
            
            config.StorageProviderName = StorageProviders.AzureTableStorage;
            config.Container.Register(cloudTable);
            return config;
        }
    }
}