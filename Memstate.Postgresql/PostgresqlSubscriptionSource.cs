using System;
using System.Threading;
using Newtonsoft.Json;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlSubscriptionSource : IJournalSubscriptionSource
    {
        private readonly PostgresqlSettings _settings;
        private readonly ISerializer _serializer;
        private readonly RingBuffer<JournalRecord> _buffer = new RingBuffer<JournalRecord>(1024);
        
        private Thread _listenerThread;

        public PostgresqlSubscriptionSource(Settings config, PostgresqlSettings settings)
        {
            _serializer = config.CreateSerializer();
            _settings = settings;
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            Listen(handler);
            // Accept incoming
            // Catch up
            // Check if last buffered command is read via catch up
            // Read buffered commands
            // Ready
            // Process incoming

            throw new NotImplementedException();
        }

        private void Listen(Action<JournalRecord> handler)
        {
            var thread = new Thread(() =>
            {
                using (var connection = new NpgsqlConnection(_settings.ConnectionString))
                {
                    connection.Open();

                    connection.Notification += (sender, arguments) =>
                    {
                        var storedCommand = JsonConvert.DeserializeObject<StoredCommand>(arguments.AdditionalInformation);

                        //var command = _settings.Serializer.Deserialize<Command>(storedCommand.Data);
                        
                       //var journalRecord = new JournalRecord(storedCommand.Id, storedCommand.);
                    };

                    while (_listenerThread.IsAlive)
                    {
                        connection.Wait();
                    }
                }
            });

            thread.Start();

            _listenerThread = thread;
        }

        public class PostgresqlCommandSubscription : IJournalSubscription
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public bool Ready()
            {
                throw new NotImplementedException();
            }
        }

        public class StoredCommand
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("command_id")]
            public Guid CommandId { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("data")]
            public byte[] Data { get; set; }
        }
    }
}