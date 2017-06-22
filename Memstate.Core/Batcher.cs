using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate.Core
{
    public class Batcher<T> : IAccept<T>, IDisposable
    {
        private readonly BlockingCollection<T> _queue = new BlockingCollection<T>();
        private readonly IAccept<T[]> _handler;
        private readonly int _batchSize;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Task _task;

        public Batcher(int batchSize, IAccept<T[]> handler)
        {
            _batchSize = batchSize;
            _handler = handler;
            _task = Task.Factory.StartNew(QueueConsumer);
        }

        public void Accept(T item)
        {
            _queue.Add(item);
        }

        private void QueueConsumer()
        {
            var buffer = new List<T>(_batchSize);
            var cancellationToken = _cancellationTokenSource.Token;
            
            while (true)
            {
                T item;
                
                if (_queue.IsCompleted && _queue.Count == 0)
                {
                    break;
                }
                
                //wait for a first item
                if (!_queue.TryTake(out item, -1, cancellationToken))
                {
                    continue;
                }
                
                buffer.Add(item);

                //take the rest but don't wait
                while (buffer.Count < _batchSize && _queue.TryTake(out item))
                {
                    buffer.Add(item);
                }

                //at this point we have at least one request to process
                var commands = buffer.ToArray();
                
                buffer.Clear();
                _handler.Accept(commands);
            }
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _cancellationTokenSource.Cancel();
            _task.Wait();
        }
    }
}