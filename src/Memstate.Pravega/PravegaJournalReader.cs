using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Memstate.Configuration;

namespace Memstate.Pravega
{
    public class PravegaJournalReader : IJournalReader
    {
        private readonly CancellationToken _cancellationToken;
        private readonly CancellationTokenSource _cts;
        
        private readonly PravegaGateway.PravegaGatewayClient _client;
        private readonly ISerializer _serializer;

        private readonly string _scope;
        private readonly string _streamName = "mystream";

        public PravegaJournalReader(PravegaGateway.PravegaGatewayClient client, ISerializer serializer)
        {
            _client = client;
            _cts = new CancellationTokenSource();
            _cancellationToken = _cts.Token;
            _serializer = serializer;
            _scope = Config.Current.GetSettings<EngineSettings>().StreamName;
        }
        public Task DisposeAsync()
        {
            _cts.Cancel();
            _cts.Token.WaitHandle.WaitOne();
            return Task.CompletedTask;
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            var request = new ReadEventsRequest
            {
                Scope = _scope,
                Stream = _streamName,
            };

            var recordNumber = 0;
            var response = _client.ReadEvents(request, cancellationToken: _cancellationToken);
            while (!_cancellationToken.IsCancellationRequested)
            {
                var responseStream = response.ResponseStream;
                while (responseStream.MoveNext().GetAwaiter().GetResult())
                {
                    if (recordNumber <= fromRecord) continue;
                    var @event = responseStream.Current;
                    var bytes = @event.Event.ToByteArray();
                    Console.WriteLine("Position:" + @event.Position);
                    Console.WriteLine("StreamCut:" + @event.StreamCut);
                    Console.WriteLine("EventPointer:" + @event.EventPointer);

                    var savedRecord = (JournalRecord) _serializer.Deserialize(bytes);
                    var record = new JournalRecord(recordNumber++, savedRecord.Written, savedRecord.Command);
                    yield return record;
                }
            }
        }
    }
}