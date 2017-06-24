using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Memstate.Core
{
    public class InMemoryCommandStore : IHandle<Command>, ICommandSubscriptionSource
    {
        class Subscription : IHandle<JournalEntry>, ICommandSubscription
        {
            public readonly Guid Id = Guid.NewGuid();
            private readonly Action<JournalEntry> _callback;
            public long NextChunkId;
            private readonly Action<Subscription> _onDisposed;

            public Subscription(Action<JournalEntry> callback, long nextChunkId, Action<Subscription> onDisposed)
            {
                NextChunkId = nextChunkId;
                _callback = callback;
                _onDisposed = onDisposed;
            }

            public void Dispose()
            {
                _onDisposed.Invoke(this);
            }

            public bool Ready()
            {
                return true;
            }

            public void Handle(JournalEntry chunk)
            {
                _callback.Invoke(chunk);
                NextChunkId++;
            }
        }

        private readonly Dictionary<Guid, Subscription> _subscriptions;
        private readonly BatchingCommandLogger _batchingLogger;
        private ulong _nextChunkSequenceNumber;
        private readonly List<CommandChunk> _chunks = new List<CommandChunk>();
        private readonly ConcurrentQueue<CommandChunk> _incomingChunks 
            = new ConcurrentQueue<CommandChunk>();

        public InMemoryCommandStore(Guid engineId, ulong nextSequence = 1)
        {
            _batchingLogger = new BatchingCommandLogger(OnCommandBatch, 100);
            _subscriptions = new Dictionary<Guid, Subscription>();
            _nextChunkSequenceNumber = nextSequence;
        }

        public void Dispose()
        {
            _batchingLogger.Dispose();
        }

        private void OnCommandBatch(Command[] commands)
        {
            var chunk = new CommandChunk(commands);
            chunk.LocalSequenceNumber = _nextChunkSequenceNumber++;
            _incomingChunks.Enqueue(chunk);
        }

        public void Handle(Command command)
        {
            _batchingLogger.Handle(command);
        }

        private void RemoveSubscription(Subscription subscription)
        {
            lock (_subscriptions)
            {
                _subscriptions.Remove(subscription.Id);
            }
        }

        public ICommandSubscription Subscribe(long from, Action<JournalEntry> handler)
        {
            var subscription = new Subscription(handler, from, RemoveSubscription);
            lock (_subscriptions)
            {
                _subscriptions.Add(subscription.Id, subscription);
            }
            return subscription;
        }
    }
}