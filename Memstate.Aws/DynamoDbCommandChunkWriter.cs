using System;
using Amazon.DynamoDBv2;
using Memstate.Core;

namespace Memstate.Aws
{
    public class DynamoDbCommandChunkWriter : IAccept<CommandChunk>, IDisposable
    {
        private readonly AmazonDynamoDBClient _client;

        public DynamoDbCommandChunkWriter(AmazonDynamoDBClient client)
        {
            _client = client;
        }
        
        public void Accept(CommandChunk item)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}