using System.Collections.Generic;

namespace Memstate
{
    public abstract class BatchingJournalWriter : IJournalWriter
    {
        private readonly Batcher<Command> _batcher;

        protected BatchingJournalWriter(Config config)
        {
            _batcher = new Batcher<Command>(config);
            _batcher.OnBatch += OnCommandBatch;

        }

        public void Send(Command command)
        {
            _batcher.Add(command);
        }

        protected abstract void OnCommandBatch(IEnumerable<Command> commands);

        public virtual void Dispose()
        {
            _batcher.Dispose();
            _batcher.OnBatch -= OnCommandBatch;
        }
    }
}