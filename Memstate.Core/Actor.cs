using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate.Core
{

    public abstract class Actor : Actor<object> { }

    public abstract class Actor<T> : IDisposable
    {
        private readonly BlockingCollection<T> _inbox;
        private readonly Task _processor;
        private readonly CancellationTokenSource _cts ;

        public void Send(T item)
        {
            _inbox.Add(item);
        }

        protected Actor(int boundedCapacity = Int32.MaxValue)
        {
            _inbox = new BlockingCollection<T>(boundedCapacity);
            _cts = new CancellationTokenSource();
            _processor = Task.Run(() => ProcessItems());
        }

        protected virtual void ProcessItems()
        {
            var cancellationToken = _cts.Token;
            while (!_inbox.IsCompleted)
            {
                if (_inbox.TryTake(out var item, -1, cancellationToken))
                {
                    Handle(item);
                }
            }
        }

        protected abstract void Handle(T item);

        public void Dispose()
        {
            _inbox.CompleteAdding();
            _cts.Cancel();
            _processor.Wait();
        }
    }
}