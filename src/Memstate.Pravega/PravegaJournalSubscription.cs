using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Memstate.Configuration;

namespace Memstate.Pravega
{
    public class PravegaJournalSubscription : IJournalSubscription
    {
        private readonly Action<JournalRecord> _handler;
        private readonly IAsyncStreamReader<ReadEventsResponse> _reader;
        private readonly ISerializer _serializer;
        private readonly long _firstRecordNumberToRead;
        private Task _task;
        private readonly CancellationTokenSource _cts;

        public PravegaJournalSubscription(Action<JournalRecord> handler, IAsyncStreamReader<ReadEventsResponse> reader, long fromRecord)
        {
            _handler = handler;
            _reader = reader;
            _serializer = Config.Current.CreateSerializer();
            _firstRecordNumberToRead = fromRecord;
            _cts = new CancellationTokenSource();
        }

        public void Start() => _task = Run();
        private async Task Run()
        {
            var recordNumber = 0;
            while (await _reader.MoveNext(_cts.Token))
            {
                //Skip forward to the position we want to start reading from
                //todo: learn how to request from a specific StreamCut
                if (recordNumber < _firstRecordNumberToRead) continue;
                var eventsResponse = _reader.Current;
                var bytes = eventsResponse.Event.ToByteArray();
                var savedRecord = (JournalRecord) _serializer.Deserialize(bytes);
                var record = new JournalRecord(recordNumber++, savedRecord.Written, savedRecord.Command);
                _handler.Invoke(record);
            }
            
        }
        
        public void Dispose()
        {
            Console.WriteLine("Terminating subscription");
            _cts.Cancel();
            _task.GetAwaiter().GetResult();
            Console.WriteLine("Subscription terminated");
        }

        //TODO: figure out how to know
        public bool Ready() => true;
    }

    public static class ConfigExtensions
    {
        public static Config UsePravega(this Config config)
        {
            config.SerializerName = Serializers.Wire;
            config.StorageProviderName = StorageProviders.Pravega;
            var provider = new PravegaProvider();
            provider.Initialize();
            config.Container.Register(provider);
            return config;
        }
    }
}