using System.Collections.Generic;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public abstract class BatchingJournalWriter : IJournalWriter
    {
        private readonly Batcher<Command> _batcher;

        protected BatchingJournalWriter()
        {
            var config = Config.Current;
            var settings = config.GetSettings<EngineSettings>();
            var maxBatchSize = settings.MaxBatchSize;
            var maxBatchQueueLength = settings.MaxBatchQueueLength;
            _batcher = new Batcher<Command>(OnCommandBatch, maxBatchSize, maxBatchQueueLength);
        }

        public void Send(Command command) => _batcher.Add(command);

        public virtual Task DisposeAsync() => _batcher.DisposeAsync();

        protected abstract void OnCommandBatch(IEnumerable<Command> commands);
    }
}