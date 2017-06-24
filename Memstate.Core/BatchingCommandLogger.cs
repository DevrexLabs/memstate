using System;

namespace Memstate.Core
{
    public class BatchingCommandLogger : IHandle<Command>
    {
        private readonly Batcher<Command> _commandBatcher;

        public BatchingCommandLogger(Action<Command[]> batchHandler, int maxBatchSize = 500)
        {
            _commandBatcher = new Batcher<Command>(maxBatchSize, batchHandler);

        }
        public void Dispose()
        {
            _commandBatcher.Dispose();
        }

        public void Handle(Command command)
        {
            _commandBatcher.Handle(command);
        }
    }
}