using System;
using System.Collections.Generic;

namespace Memstate
{
    internal class EventHandlers<TModel> : IDisposable where TModel : class
    {
        private readonly Engine<TModel> _engine;

        private readonly Dictionary<Type, ConditionalHandler> _handlers
            = new Dictionary<Type, ConditionalHandler>();

        public EventHandlers(Engine<TModel> engine)
        {
            _engine = engine;
            _engine.CommandExecuted += OnCommandExecuted;
        }

        public void Dispose()
        {
            _engine.CommandExecuted -= OnCommandExecuted;
        }

        private void OnCommandExecuted(Command command, bool isLocal, IEnumerable<Event> events)
        {
            foreach(var @event in events)
            {
                if (_handlers.TryGetValue(@event.GetType(), out var eventSubscription))
                {
                    eventSubscription.Invoke(@event);
                }
            }
        }

        public void SetHandler<T>(Action<T> handler, IEventFilter filter = null) where T : Event
        {
            _handlers[typeof(T)] = new ConditionalHandler(e => handler.Invoke((T)e), filter);
        }

        public void ClearHandler<T>() where T : Event
        {
            _handlers.Remove(typeof(T));
        }

        private class ConditionalHandler
        {
            private readonly Action<Event> _handler;

            private readonly Func<Event, bool> _matches;

            public ConditionalHandler(Action<Event> handler, IEventFilter filter = null)
            {
                _handler = handler;
                if (filter == null) _matches = Yes;
                else _matches = filter.Accept;
            }

            public void Invoke(Event item)
            {
                if (_matches.Invoke(item)) _handler(item);
            }

            private static Func<Event, bool> Yes = e => true;
        }
    }
}