using System;
using System.IO;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Memstate.Core;

namespace Memstate.Aws
{
    public class KinesisCommandChunkWriter : IHandle<CommandChunk>, IDisposable
    { 
        private readonly AmazonKinesisClient _client;
        private readonly string _streamName;
        private readonly ISerializer _serializer;

        public KinesisCommandChunkWriter(
            AmazonKinesisClient client, 
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

        public async void Handle(CommandChunk chunk)
        {
            var bytes = _serializer.Serialize(chunk);
            var request = new PutRecordRequest
            {
                SequenceNumberForOrdering = chunk.LocalSequenceNumber.ToString(),
                StreamName = _streamName,
                Data = new MemoryStream(bytes),
                PartitionKey = chunk.PartitionKey
            };
            
            var response = await _client.PutRecordAsync(request);
            chunk.GlobalSequenceNumber = response.SequenceNumber;
        }
    }
}