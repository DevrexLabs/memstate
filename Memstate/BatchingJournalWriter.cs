namespace Memstate
{
    using System.Collections.Generic;

    public abstract class BatchingJournalWriter : IJournalWriter
    {
        private readonly Batcher<Command> _batcher;

        protected BatchingJournalWriter(MemstateSettings config)
        {
            Ensure.NotNull(config, nameof(config));
            _batcher = new Batcher<Command>(config, OnCommandBatch);
        }

        public void Send(Command command)
        {
            _batcher.Add(command);
        }

        public virtual void Dispose()
        {
            _batcher.Dispose();
        }

        protected abstract void OnCommandBatch(IEnumerable<Command> commands);
    }
}