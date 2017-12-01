using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate
{
    public class ClientEvents : IClientEvents
    {
        private readonly IDictionary<Type, HashSet<Handler>> _subscriptions = new Dictionary<Type, HashSet<Handler>>();

        private readonly HashSet<IEventFilter> _globalFilters = new HashSet<IEventFilter>();

        public event Action<Type, IEnumerable<IEventFilter>> SubscriptionAdded = (type, filters) => { };

        public event Action<Type> SubscriptionRemoved = type => { };

        public event Action<IEventFilter> GlobalFilterAdded = filter => { };

        public event Action<Event> Raised = item => { };

        public void Handle(IEnumerable<Event> events)
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
            
            SubscriptionAdded?.Invoke(typeof(TEvent), Array.Empty<IEventFilter>());
        }

        public void Subscribe<TEvent>(IEnumerable<IEventFilter> filters) where TEvent : Event
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                _subscriptions[typeof(TEvent)] = handlers = new HashSet<Handler>();
            }

            filters = filters.ToArray();

            handlers.Add(new Handler(Raise, filters));
            
            SubscriptionAdded?.Invoke(typeof(TEvent), filters);
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : Event
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                _subscriptions[typeof(TEvent)] = handlers = new HashSet<Handler>();
            }

            handlers.Add(new Handler(item => handler.Invoke((TEvent) item)));
            
            SubscriptionAdded?.Invoke(typeof(TEvent), Array.Empty<IEventFilter>());
        }

        public void Subscribe<TEvent>(Action<TEvent> handler, IEnumerable<IEventFilter> filters) where TEvent : Event
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                _subscriptions[typeof(TEvent)] = handlers = new HashSet<Handler>();
            }
            
            filters = filters.ToArray();

            handlers.Add(new Handler(item => handler.Invoke((TEvent) item), filters));
            
            SubscriptionAdded?.Invoke(typeof(TEvent), filters);
        }

        public void Unsubscribe<TEvent>() where TEvent : Event
        {
            _subscriptions.Remove(typeof(TEvent));
            
            SubscriptionRemoved?.Invoke(typeof(TEvent));
        }

        public void Filter(IEnumerable<IEventFilter> filters)
        {
            filters.Select(filter => new {Added = _globalFilters.Add(filter), Filter = filter})
                .Where(item => item.Added)
                .ToList()
                .ForEach(item => GlobalFilterAdded?.Invoke(item.Filter));
        }
        
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