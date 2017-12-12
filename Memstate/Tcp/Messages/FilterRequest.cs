using Newtonsoft.Json;

namespace Memstate.Tcp
{
    internal class FilterRequest : Request
    {
        public FilterRequest(IEventFilter filter)
        {
            Filter = filter;
        }

        [JsonProperty]
        public IEventFilter Filter { get; private set; }
    }
}