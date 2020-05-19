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
        private Task _task;
        private readonly CancellationTokenSource _cts;

        public PravegaJournalSubscription(Action<JournalRecord> handler, IAsyncStreamReader<ReadEventsResponse> reader)
        {
            _handler = handler;
            _reader = reader;
            _serializer = Config.Current.CreateSerializer();
            _cts = new CancellationTokenSource();
        }

        public void Start() => _task = Run();
        private async Task Run()
        {
            while (await _reader.MoveNext(_cts.Token))
            {
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