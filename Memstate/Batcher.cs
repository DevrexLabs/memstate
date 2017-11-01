namespace Memstate
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class Batcher<T> : IDisposable
    {    
        public const int DefaultMaxBatchSize = 1000;
        private readonly int _maxBatchSize;
        private readonly BlockingCollection<T> _items;
        private readonly Task _batchTask;

        private readonly ILogger _logger;

        public Batcher(Settings config, int maxBatchSize = DefaultMaxBatchSize, int? boundedCapacity = null)
        {
            _logger = config.CreateLogger<Batcher<T>>();
            _maxBatchSize = maxBatchSize;
            _items = new BlockingCollection<T>(boundedCapacity ?? int.MaxValue);
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
                try
                {
                    buffer.Add(_items.Take());
                }
                catch (InvalidOperationException)
                {
                }

                while (buffer.Count < _maxBatchSize && _items.TryTake(out var item))
                {
                    buffer.Add(item);
                }

                if (buffer.Count > 0)
                {
                    OnBatch?.Invoke(buffer);
                    buffer.Clear();
                }
            }
        }
    }
}