using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memstate.Core
{
    public class Batcher<T> : IDisposable
    {
        public delegate void BatchHandler(IEnumerable<T> items);

        public event BatchHandler OnBatch = delegate { };

        public const int DefaultMaxBatchSize = 1000;
        public const int DefaultBoundedCapacity = 10000;
        private readonly int _maxBatchSize;
        private readonly BlockingCollection<T> _items;
        private readonly Task _batchTask;

        public Batcher(int maxBatchSize = DefaultMaxBatchSize, int boundedCapacity = DefaultBoundedCapacity)
        {
            _maxBatchSize = maxBatchSize;
            _items = new BlockingCollection<T>(boundedCapacity);
            _batchTask = Task.Run(() => ProcessItems());
        }

        public void Add(T item)
        {
            _items.Add(item);
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
                catch (InvalidOperationException) { }
                while (buffer.Count < _maxBatchSize && _items.TryTake(out var item))
                {
                    buffer.Add(item);
                }
                OnBatch.Invoke(buffer);
                buffer.Clear();
            }
        }

        public void Dispose()
        {
            _items.CompleteAdding();
            _batchTask.Wait();
        }
    }
}