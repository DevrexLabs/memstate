using System;
using System.IO;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Memstate.Core;

namespace Memstate.Aws
{
    public class KinesisCommandChunkWriter : IAccept<CommandChunk>, IDisposable
    { 
        private readonly AmazonKinesisClient _client;
        private readonly String _streamName;
        private readonly ISerializer _serializer;

        public KinesisCommandChunkWriter(AmazonKinesisClient client, 
            string streamName, ISerializer serializer)
        {
            _client = client;
            _streamName = streamName;
            _serializer = serializer;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async void Accept(CommandChunk chunk)
        {
            var bytes = _serializer.Serialize(chunk);
            var request = new PutRecordRequest
            {
                SequenceNumberForOrdering = chunk.EngineSequenceNumber.ToString(),
                StreamName = _streamName,
                Data = new MemoryStream(bytes),
                PartitionKey = chunk.Engine.ToString() //todo: ordering guarantees?
            };
            var response = await _client.PutRecordAsync(request);
            var number = response.SequenceNumber;
        }
    }
}
