using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Memstate.Configuration;

namespace Memstate.Pravega
{
    public class PravegaJournalWriter : BatchingJournalWriter
    {
        private readonly PravegaGateway.PravegaGatewayClient _client;
        private readonly ISerializer _serializer;
        private readonly AsyncClientStreamingCall<WriteEventsRequest, WriteEventsResponse> _writer;
        private readonly string _scope;
        private readonly string _stream;

        public PravegaJournalWriter(Config config, PravegaGateway.PravegaGatewayClient client, string scope, string stream)
            :base(config.GetSettings<EngineSettings>())
        {
            _serializer = config.CreateSerializer();
            _client = client;
            _scope = scope;
            _stream = stream;
            _writer = _client.WriteEvents();
        }

        public override Task DisposeAsync()
        {
            _writer.Dispose();
            return Task.CompletedTask;
        }


        protected override async Task OnCommandBatch(IEnumerable<Command> commands)
        {
            foreach(var command in commands)
            {
                var record = new JournalRecord(0, DateTimeOffset.Now, command);
                var bytes = _serializer.Serialize(record);

                var request = new WriteEventsRequest
                {
                    Event = ByteString.CopyFrom(bytes),
                    Stream = _stream,
                    Scope = _scope
                };

                await _writer.RequestStream.WriteAsync(request).NotOnCapturedContext();
            }

            await _writer.RequestStream.CompleteAsync().NotOnCapturedContext();
        }
    }
}