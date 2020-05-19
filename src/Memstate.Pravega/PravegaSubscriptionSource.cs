using System;
using Memstate.Configuration;

namespace Memstate.Pravega
{
    public class PravegaSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly PravegaGateway.PravegaGatewayClient _client;

        public PravegaSubscriptionSource(PravegaGateway.PravegaGatewayClient client)
        {
            _client = client;
        }

        public IJournalSubscription Subscribe(long @from, Action<JournalRecord> handler)
        {
            var request = new ReadEventsRequest();
            request.Scope = Config.Current.GetSettings<EngineSettings>().StreamName;
            request.Stream = "mystream";
            var response =  _client.ReadEvents(request);
            var streamReader = response.ResponseStream;
            var sub = new PravegaJournalSubscription(handler, streamReader, @from);
            sub.Start();
            return sub;
        }
    }
}