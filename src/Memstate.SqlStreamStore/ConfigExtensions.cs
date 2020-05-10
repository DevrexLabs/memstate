using Memstate.Configuration;
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
        /// <returns></returns>
        public static Config UseSqlStreamStore(this Config config, IStreamStore streamStore = null)
        {
            config.StorageProviderName = StorageProviders.SqlStreamStore;
            config.Container.Register(streamStore);
            return config;
        }
    }
}