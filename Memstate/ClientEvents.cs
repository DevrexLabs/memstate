using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate
{
    public class ClientEvents<TModel> : IClientEvents where TModel : class
    {
        private readonly IDictionary<Type, HashSet<Handler>> _subscriptions = new Dictionary<Type, HashSet<Handler>>();

        private readonly HashSet<IEventFilter> _globalFilters = new HashSet<IEventFilter>();

        public ClientEvents(Engine<TModel> engine)
        {
            engine.CommandExecuted += OnEngineOnCommandExecuted;
        }

        private void OnEngineOnCommandExecuted(JournalRecord record, bool local, IEnumerable<Event> events)
        {
            foreach (var item in events)
            {
                if (item == null)
                {
                    continue;
                }

                if (_globalFilters.Count > 0 && !_globalFilters.All(filter => filter.Accept(item)))
                {
                    continue;
                }

                if (_subscriptions.Count > 0)
                {
                    if (!_subscriptions.TryGetValue(item.GetType(), out var handlers))
                    {
                        continue;
                    }

                    foreach (var handler in handlers)
                    {
                        try
                        {
                            handler.Invoke(item);
                        }
                        catch
                        {
                            // TODO: Log the exception...
                        }
                    }
                }
                else
                {
                    Raise(item);
                }
            }
        }

        public void Subscribe<TEvent>() where TEvent : Event
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                _subscriptions[typeof(TEvent)] = handlers = new HashSet<Handler>();
            }

            handlers.Add(new Handler(Raise));
        }

        public void Subscribe<TEvent>(IEnumerable<IEventFilter> filters) where TEvent : Event
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                _subscriptions[typeof(TEvent)] = handlers = new HashSet<Handler>();
            }

            handlers.Add(new Handler(Raise, filters));
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : Event
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                _subscriptions[typeof(TEvent)] = handlers = new HashSet<Handler>();
            }

            handlers.Add(new Handler(item => handler.Invoke((TEvent) item)));
        }

        public void Subscribe<TEvent>(Action<TEvent> handler, IEnumerable<IEventFilter> filters) where TEvent : Event
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                _subscriptions[typeof(TEvent)] = handlers = new HashSet<Handler>();
            }

            handlers.Add(new Handler(item => handler.Invoke((TEvent) item), filters));
        }

        public void Unsubscribe<TEvent>() where TEvent : Event
        {
            _subscriptions.Remove(typeof(TEvent));
        }

        public void Filter(IEnumerable<IEventFilter> filters)
        {
            filters.ToList().ForEach(filter => _globalFilters.Add(filter));
        }

        public event Action<Event> Raised = e => { };

        private void Raise(Event item)
        {
            Raised?.Invoke(item);
        }
        
        private class Handler
        {
            private readonly Action<Event> _action;
            private readonly HashSet<IEventFilter> _filters;

            public Handler(Action<Event> action, IEnumerable<IEventFilter> filters)
            {
                Ensure.NotNull(action, nameof(action));
                Ensure.NotNull(filters, nameof(filters));

                _action = action;
                _filters = new HashSet<IEventFilter>(filters);
            }

            public Handler(Action<Event> action)
                : this(action, Array.Empty<IEventFilter>())
            {
            }

            public override bool Equals(object obj)
            {
                switch (obj)
                {
                    case Handler handler:
                        return handler._action.Equals(_action) &&
                               handler._filters.All(filter => _filters.Contains(filter));

                    default:
                        return false;
                }
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return _filters.Aggregate(17 * 31 + _action.GetHashCode(), (current, filter) => current * 31 + filter.GetHashCode());
                }
            }

            public void Invoke(Event item)
            {
                _action(item);
            }
        }
    }
}