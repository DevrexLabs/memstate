using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Memstate;
using Memstate.JsonNet;

namespace EventStore.Tests
{
    public class EnginePoc<M>
    {
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<object>> _pendingCommands 
            = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();

        private readonly IEventStoreConnection _eventStore;
        private readonly BlockingCollection<Command> _commandWriteBuffer = new BlockingCollection<Command>();
        private readonly string _streamName = "memstate-poc-" + Guid.NewGuid();
        private readonly ISerializer _serializer = new JsonSerializerAdapter();
        private readonly Task _writerTask;
        private readonly EventStoreCatchUpSubscription _subscription;

        public int CommandsReceived { get; private set; }
        
        public EnginePoc(IEventStoreConnection eventStore)
        {
            _eventStore = eventStore;
            _writerTask = Task.Run((Action)CommandWriter);
            var settings = new CatchUpSubscriptionSettings(1000, 512, true, false);
            _subscription = _eventStore.SubscribeToStreamFrom(_streamName, null, settings, EventAppeared);
        }

        private void EventAppeared(EventStoreCatchUpSubscription sub, ResolvedEvent @event)
        {
            var bytes = @event.Event.Data;
            var command = (Command) _serializer.Deserialize(bytes);
            CommandsReceived++;
            if (_pendingCommands.TryRemove(command.Id, out var completionSource))
            {
                completionSource.SetResult(CommandsReceived);
            }
        }

        public void Dispose()
        {
            _commandWriteBuffer.CompleteAdding();
            _writerTask.Wait();
            while (!_pendingCommands.IsEmpty) Thread.Sleep(10);
            _subscription.Stop();
        }
        private void CommandWriter()
        {
            var buffer = new List<Command>();
            while (!_commandWriteBuffer.IsCompleted)
            {
                try
                {
                    buffer.Add(_commandWriteBuffer.Take());
                }
                catch (InvalidOperationException)
                {
                }
                while (buffer.Count < 10 && _commandWriteBuffer.TryTake(out Command command))
                {
                    buffer.Add(command);
                }
                if (buffer.Count > 0)
                {
                    _eventStore.AppendToStreamAsync(_streamName, ExpectedVersion.Any,
                        buffer.Select(CommandToEventData).ToArray());
                    buffer.Clear();
                }
            }
        }

        private EventData CommandToEventData(Command command)
        {
            return new EventData(Guid.NewGuid(), "poc", true, _serializer.Serialize(command), null);
        }

        public async Task<R> ExecuteAsync<R>(Command<M, R> command)
        {
            var tcs = new TaskCompletionSource<object>();
            _pendingCommands[command.Id] = tcs;
            _commandWriteBuffer.Add(command);
            return (R) await tcs.Task;
        }
    }
}