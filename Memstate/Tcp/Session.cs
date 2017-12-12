using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Memstate.Tcp
{
    /// <summary>
    /// Handles incoming messages and emits outgoing messages 
    /// for a given client connection.
    /// </summary>
    internal class Session<T> : IHandle<Message> where T : class
    {
        private readonly Engine<T> _engine;

        private readonly ILogger _logger;

        private readonly Dictionary<Type, IEventFilter[]> _subscriptions = new Dictionary<Type, IEventFilter[]>();

        private readonly HashSet<IEventFilter> _globalFilters = new HashSet<IEventFilter>();

        public Session(MemstateSettings config, Engine<T> engine)
        {
            _engine = engine;
            _logger = config.LoggerFactory.CreateLogger<Session<T>>();

            _engine.CommandExecuted += (record, local, events) =>
            {
                var filteredEvents = from e in events
                    let filters = _subscriptions.GetOrDefault(e.GetType(), Array.Empty<IEventFilter>())
                    where _globalFilters.All(f => f.Accept(e)) && (_subscriptions.Count == 0 || filters.Any() && filters.All(f => f.Accept(e)))
                    select e;

                var message = new EventsResponse(filteredEvents.ToArray());

                OnMessage.Invoke(message);
            };
        }

        public event Action<Message> OnMessage = _ => { };

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

                    case FilterRequest request:
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
            _subscriptions[request.Type] = request.Filters;

            OnMessage.Invoke(new SubscribeResponse(request.Id));
        }

        private void HandleImpl(UnsubscribeRequest request)
        {
            _subscriptions.Remove(request.Type);

            OnMessage.Invoke(new UnsubscribeResponse(request.Id));
        }

        private void HandleImpl(FilterRequest request)
        {
            _globalFilters.Add(request.Filter);

            OnMessage.Invoke(new FilterResponse(request.Id));
        }
    }
}