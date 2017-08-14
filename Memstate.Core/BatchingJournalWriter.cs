using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memstate.Core
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

        protected abstract Task OnCommandBatch(IEnumerable<Command> commands);

        public virtual void Dispose()
        {
            _batcher.Dispose();
            _batcher.OnBatch -= OnCommandBatch;
        }
    }
}