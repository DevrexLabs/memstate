using System;
using System.Collections.Generic;

namespace Memstate
{
    public interface IClientEvents
    {
        /// <summary>
        /// Subscribe to events of a specific type.
        /// </summary>
        /// <typeparam name="TEvent">
        /// The event type.
        /// </typeparam>
        void Subscribe<TEvent>() where TEvent : Event;
        
        /// <summary>
        /// Subscribe to events of a specific type that matches the filters.
        /// </summary>
        /// <param name="filters">
        /// The filters.
        /// </param>
        /// <typeparam name="TEvent">
        /// The event type.
        /// </typeparam>
        void Subscribe<TEvent>(IEnumerable<IEventFilter> filters) where TEvent : Event;
        
        /// <summary>
        /// Subscribe to events of a specific type and handle them with the supplied handler.
        /// </summary>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <typeparam name="TEvent">
        /// The event type.
        /// </typeparam>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : Event;
        
        /// <summary>
        /// Subscribe to events of a specific type that matches the filters and handle them with the supplied handler.
        /// </summary>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <param name="filters">
        /// The filters.
        /// </param>
        /// <typeparam name="TEvent">
        /// The event type.
        /// </typeparam>
        void Subscribe<TEvent>(Action<TEvent> handler, IEnumerable<IEventFilter> filters) where TEvent : Event;
        
        /// <summary>
        /// Unsubscribe to all events of a specific.
        /// </summary>
        /// <typeparam name="TEvent">
        /// The event type.
        /// </typeparam>
        void Unsubscribe<TEvent>() where TEvent : Event;
        
        /// <summary>
        /// Filter all incoming events via the supplied filters.
        /// </summary>
        /// <param name="filters">
        /// The filters.
        /// </param>
        void Filter(IEnumerable<IEventFilter> filters);
        
        /// <summary>
        /// Raised is invoked when an event is raised.
        /// </summary>
        event Action<Event> Raised;
    }
}