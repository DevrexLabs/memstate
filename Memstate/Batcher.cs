namespace Memstate
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class Batcher<T> : IDisposable
    {    
        private readonly int _maxBatchSize;
        private readonly BlockingCollection<T> _items;
        private readonly Task _batchTask;

        private readonly ILogger _logger;

        public Batcher(MemstateSettings config)
        {
            _logger = config.CreateLogger<Batcher<T>>();
            _maxBatchSize = config.MaxBatchSize;
            _items = new BlockingCollection<T>(config.MaxBatchQueueLength);
            _batchTask = Task.Run((Action)ProcessItems);
        }

        public delegate void BatchHandler(IEnumerable<T> items);

        public event BatchHandler OnBatch;

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void Dispose()
        {
            _logger.LogDebug("Begin Dispose");
            _items.CompleteAdding();
            _batchTask.Wait();
            _logger.LogDebug("End Dispose");
        }

        private void ProcessItems()
        {
            var buffer = new List<T>(_maxBatchSize);
            while (!_items.IsCompleted)
            {
                if (_items.TryTake(out var firstItem, 1000))
                {
                    buffer.Add(firstItem);
                    while (buffer.Count < _maxBatchSize && _items.TryTake(out var item))
                    {
                        buffer.Add(item);
                    }

                    OnBatch?.Invoke(buffer);
                    buffer.Clear();
                }
            }
        }
    }
}