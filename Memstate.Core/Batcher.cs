using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Memstate.Core
{
    public class Batcher<T> : IDisposable
    {
        public delegate Task BatchHandler(IEnumerable<T> items);

        public event BatchHandler OnBatch;
    
        public const int DefaultMaxBatchSize = 1000;
        private readonly int _maxBatchSize;
        private readonly BlockingCollection<T> _items;
        private readonly Task _batchTask;

        private readonly ILogger _logger = Logging.CreateLogger<Batcher<T>>();

        public Batcher(int maxBatchSize = DefaultMaxBatchSize, int? boundedCapacity = null)
        {
            _maxBatchSize = maxBatchSize;
            _items = new BlockingCollection<T>(boundedCapacity ?? Int32.MaxValue);
            _batchTask = Task.Run(ProcessItems);
        }

        public void Add(T item)
        {
            _items.Add(item);
        }

        
        private async Task ProcessItems()
        {
            var buffer = new List<T>(_maxBatchSize);
            while (!_items.IsCompleted)
            {
                try
                {
                    buffer.Add(_items.Take());
                }
                catch (InvalidOperationException) { }
                while (buffer.Count < _maxBatchSize && _items.TryTake(out var item))
                {
                    buffer.Add(item);
                }
                if (buffer.Count > 0 && OnBatch != null) await OnBatch.Invoke(buffer);
                buffer.Clear();
            }
        }

        public void Dispose()
        {
            _logger.LogDebug("Begin Dispose");
            _items.CompleteAdding();
            //_batchTask.Wait();
            _logger.LogDebug("End Dispose");
        }
    }
}