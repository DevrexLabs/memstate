using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memstate
{
    public abstract class BatchingJournalWriter : IJournalWriter
    {
        private readonly Batcher<Command> _batcher;

        protected BatchingJournalWriter()
        {
            _batcher = new Batcher<Command>(OnCommandBatch);
        }

        public void Send(Command command) => _batcher.Add(command);

        public virtual Task DisposeAsync() => _batcher.DisposeAsync();

        protected abstract void OnCommandBatch(IEnumerable<Command> commands);
    }
}