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

        public TableStorageProvider()
        {
            try
            {
                var config = Config.Current;
                var engineSettings = config.GetSettings<EngineSettings>();
                if (!config.Container.TryResolve(out CloudTable cloudTable)) throw new Exception("No CloudTable configured, did you forget to call Config.Current.UseAzureTableStorage()?");
                _partition = new Partition(cloudTable, engineSettings.StreamName);
                cloudTable.CreateIfNotExists();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to initialize Azure TableStorageProvider, see inner exception for details", e);
            }
        }

        public Task Provision()
        {
            //todo: Move cloudTable.CreateIfNotExists here
            return Task.CompletedTask;
        }

        public IJournalReader CreateJournalReader()
        {
            return new TableStorageJournalReader(_partition);
        }

        public IJournalWriter CreateJournalWriter()
        {
            var head = Stream.OpenAsync(_partition).GetAwaiter().GetResult();
            return new TableStorageJournalWriter(head);
        }
    }
}