using System;

namespace Memstate
{
    public interface IEventFilter
    {
        bool Accept(Event item);
    }

    public class EventByType : IEventFilter
    {

        public EventByType(Type type)
        {
            Type = type;
        }

        public Type Type { get; private set; }

        public virtual bool Accept(Event @event)
        {
            return @event.GetType() == Type;
        }
    }
}