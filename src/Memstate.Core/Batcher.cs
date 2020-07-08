using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memstate.Logging;

namespace Memstate
{
    internal class Batcher<T> : IAsyncDisposable
    {
        private readonly int _maxBatchSize;

        private readonly BlockingCollection<T> _items;

        private readonly Task _batchTask;

        private readonly Func<IEnumerable<T>, Task> _batchHandler;

        private readonly ILog _logger;

        public Batcher(Func<IEnumerable<T>, Task> batchHandler, int maxBatchSize, int maxQueueLength)
        {
            _logger = LogProvider.GetCurrentClassLogger();
            _batchHandler = batchHandler;
            _maxBatchSize = maxBatchSize;
            _items = new BlockingCollection<T>(maxQueueLength);
            _batchTask = Task.Run(ProcessItems);
        }

        public void Add(T item) => _items.Add(item);

        public async Task DisposeAsync()
        {
            _logger.Debug("Begin Dispose");
            _items.CompleteAdding();
            await _batchTask.NotOnCapturedContext();
            _logger.Debug("End Dispose");
        }

        private async Task ProcessItems()
        {
            var buffer = new List<T>(_maxBatchSize);

            while (!_items.IsCompleted)
            {
                //This will only effect the time it takes to Dispose
                const int waitTimeInMillis = 1000;

                //Wait for an item to arrive
                if (_items.TryTake(out var firstItem, waitTimeInMillis))
                {
                    buffer.Add(firstItem);

                    //then take items as long as they are available or more items until we reach 
                    while (buffer.Count < _maxBatchSize && _items.TryTake(out var item))
                    {
                        buffer.Add(item);
                    }

                    await _batchHandler.Invoke(buffer);
                    buffer.Clear();
                }
            }
        }
    }
}