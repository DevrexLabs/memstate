using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Memstate.Configuration;
using Memstate.Logging;


namespace Memstate.EventStore
{
    public class EventStoreReader : JournalReader
    {
        private readonly IEventStoreConnection _connection;

        private readonly ISerializer _serializer;

        private readonly string _streamName;

        private readonly int _eventsPerSlice;

        private readonly ILog _logger;

        public EventStoreReader(IEventStoreConnection connection)
        {
            var config = Config.Current;
            var settings = config.GetSettings<EngineSettings>();
            var eventStoreSettings = config.GetSettings<EventStoreSettings>();
            _logger = LogProvider.GetCurrentClassLogger();
            _connection = connection;
            _serializer = config.Serializers.Resolve(eventStoreSettings.SerializerName);
            _streamName = eventStoreSettings.StreamName;
            _eventsPerSlice = eventStoreSettings.EventsPerSlice;
        }

        public override Task Subscribe(long first, long last, Action<JournalRecord> recordHandler, CancellationToken cancellationToken)
        {
            long? checkPoint = null;
            if (first > 0) checkPoint = first - 1;
            
            var completionSource = new TaskCompletionSource<object>();
            
            var sub = _connection.SubscribeToStreamFrom(
                stream: _streamName,
                lastCheckpoint: checkPoint,
                settings: new CatchUpSubscriptionSettings(10000, 4096, false, false),
                eventAppeared: (s, re) =>
                {
                    _logger.Debug("eventAppeared, recordNumber {0}", re.OriginalEventNumber);
                    recordHandler.Invoke(re.Event.ToJournalRecord(_serializer));
                    if (re.OriginalEventNumber == last)
                    {
                        s.Stop();
                        completionSource.SetResult(0);
                    }
                }, 
                subscriptionDropped: (s, r, e) =>
                {
                    _logger.ErrorException("ES Subscription dropped, reason: " + r, e);
                    completionSource.SetException(e);
                });
            
            cancellationToken.Register(() =>
            {
                sub.Stop();
                completionSource.SetResult(0);
            });
 
            return completionSource.Task;
        }

        public override IEnumerable<JournalRecord> ReadRecords(long fromRecord)
        {
            var nextRecord = fromRecord;

            _logger.Info("GetRecords from {0}", nextRecord);

            while (true)
            {
                var slice = _connection.ReadStreamEventsForwardAsync(_streamName, nextRecord, _eventsPerSlice, false).Result;

                _logger.Debug("{0} events in slice from {0}", slice.Events.Length, slice.FromEventNumber);

                foreach (var @event in slice.Events.Select(e => e.Event))
                {
                    yield return @event.ToJournalRecord(_serializer);
                }

                if (slice.IsEndOfStream)
                {
                    break;
                }

                nextRecord = slice.NextEventNumber;
            }

            _logger.Info("GetRecords completed");
        }

        public override Task DisposeAsync() => Task.CompletedTask;
    }
}