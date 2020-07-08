using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PravegaClient = PravegaGateway.PravegaGatewayClient;

namespace Memstate.Pravega
{
    public class PravegaJournalReader : JournalReader
    {
        private readonly CancellationToken _cancellationToken;
        private readonly CancellationTokenSource _cts;

        private readonly PravegaClient _client;
        private readonly ISerializer _serializer;

        private readonly string _scope;
        private readonly string _stream = "mystream";

        private readonly Action<StreamCut> _lastEventReadHandler;

        public PravegaJournalReader(PravegaClient client, ISerializer serializer, string scope, string stream, Action<StreamCut> lastEventRead)
        {
            _client = client;
            _cts = new CancellationTokenSource();
            _cancellationToken = _cts.Token;
            _serializer = serializer;
            _scope = scope;
            _stream = stream;
            _lastEventReadHandler = lastEventRead;
        }

        public override Task DisposeAsync()
        {
            _cts.Cancel();
            _cts.Token.WaitHandle.WaitOne();
            return Task.CompletedTask;
        }

        public override IEnumerable<JournalRecord> ReadRecords(long fromRecord)
        {

            var endStreamCut = GetEndStreamCut();
            _lastEventReadHandler.Invoke(endStreamCut);

            var request = new ReadEventsRequest
            {
                Scope = _scope,
                Stream = _stream,
                ToStreamCut = endStreamCut
            };

            var recordNumber = 0;
            using var call = _client.ReadEvents(request, cancellationToken: _cancellationToken);

            var responseStream = call.ResponseStream;
            while (responseStream.MoveNext(_cancellationToken).GetAwaiter().GetResult())
            {
                if (recordNumber <= fromRecord) continue;
                var @event = responseStream.Current;
                var bytes = @event.Event.ToByteArray();
                var savedRecord = (JournalRecord) _serializer.Deserialize(bytes);
                var record = new JournalRecord(recordNumber++, savedRecord.Written, savedRecord.Command);
                yield return record;
            }
        }

        /// <summary>
        /// Get a reference to the current end of the stream
        /// </summary>
        /// <returns></returns>
        private StreamCut GetEndStreamCut()
        {
            var request = new GetStreamInfoRequest { Scope = _scope, Stream = _stream };
            var response = _client.GetStreamInfo(request);
            return response.TailStreamCut;
        }
    }
}