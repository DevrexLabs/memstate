using System.Collections.Generic;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public abstract class BatchingJournalWriter : IJournalWriter
    {
        private readonly Batcher<Command> _batcher;

        protected BatchingJournalWriter(EngineSettings settings)
        {
            var maxBatchSize = settings.MaxBatchSize;
            var maxBatchQueueLength = settings.MaxBatchQueueLength;
            _batcher = new Batcher<Command>(OnCommandBatch, maxBatchSize, maxBatchQueueLength);
        }

        public Task Write(Command command)
        {
            _batcher.Add(command);
            return Task.CompletedTask;
        }

        public virtual Task DisposeAsync() => _batcher.DisposeAsync();

        protected abstract Task OnCommandBatch(IEnumerable<Command> commands);
    }
}