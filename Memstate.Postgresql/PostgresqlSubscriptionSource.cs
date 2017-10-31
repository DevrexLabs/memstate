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
            _listenerThread = new Thread(
                () =>
                {
                    var serializser = _settings.CreateSerializer();

                    using (var connection = new NpgsqlConnection(_settings.ConnectionString))
                    {
                        connection.Open();

                        connection.Notification += (sender, arguments) =>
                        {
                            var row = JsonConvert.DeserializeObject<Row>(arguments.AdditionalInformation);

                            var command = (Command) serializser.Deserialize(row.Data);

                            var record = new JournalRecord(row.Id, row.Written, command);

                            try
                            {
                                handler(record);
                            }
                            catch (Exception)
                            {
                                // TODO: Log the exception.
                            }
                        };

                        while (_listenerThread.IsAlive)
                        {
                            connection.Wait();
                        }
                    }
                });

            _listenerThread.Start();
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

        public class Row
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("written")]
            public DateTime Written { get; set; }

            [JsonProperty("data")]
            public byte[] Data { get; set; }
        }
    }
}