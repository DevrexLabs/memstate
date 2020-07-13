using System;
using System.Threading.Tasks;
using Memstate.Configuration;
using Microsoft.Azure.Cosmos.Table;
using Streamstone;

namespace Memstate.Azure
{
    public class TableStorageProvider : IStorageProvider
    {
        private readonly Partition _partition;
        private readonly Config _config;

        public TableStorageProvider(Config config)
        {
            _config = config;
            try
            {
                var engineSettings = config.GetSettings<EngineSettings>();
                if (!config.Container.TryResolve(out CloudTable cloudTable)) throw new Exception("No CloudTable configured, did you forget to call Config.Current.UseAzureTableStorage()?");
                _partition = new Partition(cloudTable, engineSettings.StreamName);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to initialize Azure TableStorageProvider, see inner exception for details", e);
            }
        }

        public Task Provision() => Stream.ProvisionAsync(_partition);

        public IJournalReader CreateJournalReader()
        {
            var serializer = _config.CreateSerializer();
            return new TableStorageJournalReader(serializer, _partition);
        }

        public IJournalWriter CreateJournalWriter()
        {
            var head = Stream.OpenAsync(_partition).GetAwaiter().GetResult();
            return new TableStorageJournalWriter(_config, head);
        }
    }
}