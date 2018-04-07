namespace Memstate.Tcp
{
    internal class EventsRaised : Message
    {
        public EventsRaised(Event[] events)
        {
            Events = events;
        }

        public Event[] Events { get; private set; }
    }
}