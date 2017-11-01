using System;
using System.Data;
using System.Diagnostics;
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
            
            return new PostgresqlJournalSubscription();

            throw new NotImplementedException();
        }

        private void Listen(Action<JournalRecord> handler)
        {
            _listenerThread = new Thread(
                () =>
                {
                    using (var connection = new NpgsqlConnection(_settings.ConnectionString))
                    {
                        connection.Open();

                        SendListenCommand(connection);

                        connection.Notification += (sender, arguments) =>
                        {
                            Debug.WriteLine("Received a notification from Postgres");

                            try
                            {
                                var row = JsonConvert.DeserializeObject<Row>(arguments.AdditionalInformation);

                                var command = (Command) _serializer.Deserialize(row.Command);

                                var record = new JournalRecord(row.Id, row.Written, command);
                                
                                handler(record);
                            }
                            catch (Exception exception)
                            {
                                // TODO: Log the exception.
                                Debug.WriteLine($"Exception: {exception.Message}");
                            }
                        };

                        while (_listenerThread.IsAlive)
                        {
                            Debug.WriteLine("Waiting for incoming Postgres connections...");
                            
                            connection.Wait();
                        }
                    }
                });

            _listenerThread.Name = "Memstate:Postgresql:NotificationsListener";

            _listenerThread.Start();
        }

        private void SendListenCommand(IDbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"LISTEN {_settings.SubscriptionStream};";

                command.ExecuteNonQuery();
            }
        }

        public class PostgresqlJournalSubscription : IJournalSubscription
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

            [JsonProperty("command")]
            public byte[] Command { get; set; }
        }
    }
}