using Newtonsoft.Json;

namespace Memstate.Tcp
{
    internal class EventsResponse : Message
    {
        public EventsResponse(Event[] events)
        {
            Events = events;
        }

        [JsonProperty]
        public Event[] Events { get; private set; }
    }
}