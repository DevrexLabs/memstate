using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memstate.Core
{
    public class Batcher<T> : IDisposable
    {
        public const int DefaultMaxBatchSize = 1000;
        public const int DefaultBoundedCapacity = 10000;
        private readonly int _maxBatchSize;
        private readonly BlockingCollection<T> _items;
        private readonly Action<IEnumerable<T>> _batchHandler;
        private readonly Task _batchTask;

        public Batcher(Action<IEnumerable<T>> batchHandler, 
            int maxBatchSize = DefaultMaxBatchSize, 
            int boundedCapacity = DefaultBoundedCapacity)
        {
            _maxBatchSize = maxBatchSize;
            _items = new BlockingCollection<T>(boundedCapacity);
            _batchHandler = batchHandler;
            _batchTask = Task.Run(() => ProcessItems());
        }

        public void Append(T item)
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
                _batchHandler.Invoke(buffer);
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