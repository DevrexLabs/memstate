using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Memstate.Configuration;

namespace Memstate.Pravega
{
    public class PravegaJournalWriter : IJournalWriter
    {
        private readonly PravegaGateway.PravegaGatewayClient _client;
        private readonly ISerializer _serializer;
        private readonly AsyncClientStreamingCall<WriteEventsRequest, WriteEventsResponse> _writer;
        public PravegaJournalWriter(PravegaGateway.PravegaGatewayClient client, ISerializer serializer)
        {
            _serializer = serializer;
            _client = client;
            _writer = _client.WriteEvents();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async void Send(Command command)
        {
            var record = new JournalRecord(0, DateTimeOffset.Now, command);
            var bytes = _serializer.Serialize(record);
            
            var request = new WriteEventsRequest
            {
                Event = ByteString.CopyFrom(bytes),
                Stream = "mystream",
                Scope = Config.Current.GetSettings<EngineSettings>().StreamName
            };
            await _writer.RequestStream.WriteAsync(request);
        }
    }
}