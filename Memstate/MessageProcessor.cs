using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate
{
    /// <summary>
    /// Processes messages in the order they are received. Thread safe.
    /// Uses an unbounded blocking collection. Calling Dispose will  drain the messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageProcessor<T> : IDisposable
    {
        private readonly BlockingCollection<T> _messageQueue;

        private readonly CancellationTokenSource _cancellationSource;

        private readonly CancellationToken _cancellationToken;

        private readonly Func<T, Task> _handler;

        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public MessageProcessor(Func<T, Task> handler)
        {
            _handler = handler;
            _messageQueue = new BlockingCollection<T>();
            _cancellationSource = new CancellationTokenSource();
            _cancellationToken = _cancellationSource.Token;

            Task.Run(ProcessMessages);
        }

        public void Enqueue(T message) => _messageQueue.Add(message);

        public void Dispose()
        {
            _messageQueue.CompleteAdding();
            _cancellationSource.Cancel();
            _resetEvent.WaitOne();
            _messageQueue.Dispose();
        }

        private async Task ProcessMessages()
        {
            while (!_messageQueue.IsCompleted)
            {
                var message = _messageQueue.TakeOrDefault(_cancellationToken);

                if (message == null)
                {
                    break;
                }

                await _handler.Invoke(message).ConfigureAwait(false);
            }

            _resetEvent.Set();
        }
    }
}