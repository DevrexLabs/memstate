using System.IO;

namespace Memstate.Core
{
    public class BatchingCommandLogger : ICommandLogger
    {
        private readonly Batcher<Command> _commandBatcher;

        public BatchingCommandLogger(IAccept<Command[]> batchHandler, int maxBatchSize = 500)
        {
            _commandBatcher = new Batcher<Command>(maxBatchSize, batchHandler);

        }
        public void Dispose()
        {
            _commandBatcher.Dispose();
        }

        public void Append(Command command)
        {
            _commandBatcher.Accept(command);
        }
    }
}