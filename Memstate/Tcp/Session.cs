using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Memstate.Tcp
{
    /// <summary>
    /// Handles incoming messages and emits outgoing messages 
    /// for a given client connection.
    /// </summary>
    internal class Session<T> : IDisposable, IHandle<Message> where T : class
    {
        private readonly Engine<T> _engine;

        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<Type, EventMatcher> _subscriptions; 

        public Session(MemstateSettings config, Engine<T> engine)
        {
            _engine = engine;
            _logger = config.LoggerFactory.CreateLogger<Session<T>>();
            _engine.CommandExecuted += SendMatchingEvents;
            _subscriptions = new ConcurrentDictionary<Type, EventMatcher>();
        }

        private void SendMatchingEvents(JournalRecord journalRecord, bool isLocal, IEnumerable<Event> events)
        {
            var matchingEvents = events
                .Where(e => _subscriptions.Values.Any(matcher => matcher.IsMatch(e)))
                .ToArray();

            if (matchingEvents.Length > 0)
            {
                _logger.LogTrace("Sending {0} events", matchingEvents.Length);
                OnMessage.Invoke(new EventsRaised(matchingEvents));
            }
        }

        public event Action<Message> OnMessage = _ => { };

        public void Dispose()
        {
            _engine.CommandExecuted -= SendMatchingEvents;
        }

        public void Handle(Message message)
        {
            try
            {
                switch (message)
                {
                    case QueryRequest request:
                        HandleImpl(request);
                        break;

                    case CommandRequest request:
                        HandleImpl(request);
                        break;

                    case Ping ping:
                        HandleImpl(ping);
                        break;

                    case SubscribeRequest request:
                        HandleImpl(request);
                        break;

                    case UnsubscribeRequest request:
                        HandleImpl(request);
                        break;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message: " + message);
                OnMessage.Invoke(new ExceptionResponse(message, ex));
            }
        }

        private void HandleImpl(QueryRequest request)
        {
            var result = _engine.Execute(request.Query);
            var response = new QueryResponse(result, request.Id);

            OnMessage.Invoke(response);
        }

        private void HandleImpl(CommandRequest request)
        {
            var result = _engine.Execute(request.Command);
            var response = new CommandResponse(result, request.Id);

            OnMessage.Invoke(response);
        }

        private void HandleImpl(Ping ping)
        {
            OnMessage.Invoke(new Pong(ping));
        }

        private void HandleImpl(SubscribeRequest request)
        {
            _subscriptions[request.Type] = new EventMatcher(request.Type, request.Filter);
            OnMessage.Invoke(new SubscribeResponse(request.Id));
        }

        private void HandleImpl(UnsubscribeRequest request)
        {
            _subscriptions.TryRemove(request.Type, out var _);
            OnMessage.Invoke(new UnsubscribeResponse(request.Id));
        }

        private class EventMatcher
        {
            public Type Type { get; private set; }

            public IEventFilter Filter { get; private set; }

            public EventMatcher(Type type, IEventFilter filter)
            {
                Type = type;
                Filter = filter;
            }

            public bool IsMatch(Event @event)
            {
                return Type == @event.GetType() && Filter.Accept(@event);
            }
        }
    }
}