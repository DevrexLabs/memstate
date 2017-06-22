using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Memstate.Core;

namespace Memstate.Aws
{
    public class KinesisCommandChunkSubscriber : IDisposable
    {
        private readonly AmazonKinesisClient _client;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _streamName;
        private string _lastSequenceNumber;
        private readonly ISerializer _serializer;
        private readonly IAccept<CommandChunk> _chunkHandler;
        private readonly Task _task;

        public KinesisCommandChunkSubscriber(
            AmazonKinesisClient client,
            ISerializer serializer,
            IAccept<CommandChunk> chunkHandler,
            string streamName, string lastSequenceNumber, string shardId)
        {
            _client = client;
            _streamName = streamName;
            _chunkHandler = chunkHandler;
            _lastSequenceNumber = lastSequenceNumber;
            _cancellationTokenSource = new CancellationTokenSource();
            _serializer = serializer;
            //var shards = GetShardIds().Result;
            //if (shards.Length != 1) throw new NotSupportedException("Cannot subscribe to multishard stream, reduce to single shard");

            _task = Task.Factory.StartNew(() => Read(shardId), _cancellationTokenSource.Token);
        }

        private async Task<string> GetShardIterator(string shardId)
        {
            var request = new GetShardIteratorRequest
            {
                StreamName = _streamName,
                ShardIteratorType = ShardIteratorType.AFTER_SEQUENCE_NUMBER,
                StartingSequenceNumber = _lastSequenceNumber,
                ShardId = shardId
            };

            var response = await _client.GetShardIteratorAsync(request);

            return response.ShardIterator;
        }

        //todo: handle multiple shards, aws sdk help says one thread per shard. Is _client thread safe?
        //todo: figure out algorithm to merge sequence in correct order
        private async Task<string[]> GetShardIds()
        {
            string lastShardId = null;
            var shardIds = new List<string>();

            while (true)
            {
                var request = new DescribeStreamRequest
                {
                    StreamName = _streamName,
                    ExclusiveStartShardId = lastShardId
                };

                var stream = (await _client.DescribeStreamAsync(request)).StreamDescription;

                shardIds.AddRange(stream.Shards.Select(shard => shard.ShardId));

                if (!stream.HasMoreShards)
                {
                    break;
                }

                lastShardId = shardIds.Last();
            }

            return shardIds.ToArray();
        }

        private async void Read(string shardId)
        {
            var shardIterator = await GetShardIterator(shardId);

            while (true)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                var request = new GetRecordsRequest
                {
                    Limit = 100,
                    ShardIterator = shardIterator
                };

                var response = await _client.GetRecordsAsync(request);

                if (response.Records == null)
                {
                    break; //todo: raise exception or event
                }

                if (response.Records.Any())
                {
                    _lastSequenceNumber = response.Records.Last().SequenceNumber;

                    response.Records.ForEach(record =>
                    {
                        var chunk = (CommandChunk) _serializer.Deserialize(record.Data.ToArray());

                        _chunkHandler.Accept(chunk);
                    });
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }

                shardIterator = response.NextShardIterator;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _task.Wait();
            _client.Dispose();
        }
    }
}