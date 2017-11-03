using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlJournalSubscription : IJournalSubscription
    {
        private static readonly object Lock = new object();
        private readonly Thread _listenerThread;
        private readonly PostgresqlSettings _settings;
        private readonly MemstateSettings _memstateSettings;
        private readonly Action<JournalRecord> _handler;
        private readonly ILogger _log;
        private readonly RingBuffer<JournalRecord> _buffer = new RingBuffer<JournalRecord>(4096);

        private bool _shouldListen;
        private bool _ready;

        public PostgresqlJournalSubscription(MemstateSettings memstateSettings, Action<JournalRecord> handler)
        {
            Ensure.NotNull(memstateSettings, nameof(memstateSettings));
            _memstateSettings = memstateSettings;
            _settings = new PostgresqlSettings(memstateSettings);
            _handler = handler;

            _log = memstateSettings.LoggerFactory.CreateLogger("Memstate:Postgresql");

            _listenerThread = new Thread(Listen)
            {
                Name = "Memstate:Postgresql:NotificationsListener"
            };
        }

        public void Start()
        {
            _shouldListen = true;

            _listenerThread.Start();
        }

        public void CatchUp(long from)
        {
            var reader = new PostgresqlJournalReader(_memstateSettings);

            while (true)
            {
                var lastRecordId = from;

                var records = reader.GetRecords(lastRecordId);

                foreach (var record in records)
                {
                    lastRecordId = record.RecordNumber;

                    try
                    {
                        _handler(record);
                    }
                    catch (Exception exception)
                    {
                        _log.LogError(exception, $"Exception occured in {GetType().Name}.CatchUp");
                    }
                }

                lock (Lock)
                {
                    if (_buffer.TryPeek(out var queuedRecord))
                    {
                        if (queuedRecord.RecordNumber <= lastRecordId)
                        {
                            // Caught up
                            while (_buffer.TryDequeue(out queuedRecord))
                            {
                                try
                                {
                                    _handler(queuedRecord);
                                }
                                catch (Exception exception)
                                {
                                    _log.LogError(exception, $"Exception occured in {GetType().Name}.CatchUp");
                                }
                            }
                        }
                    }
                    else
                    {
                        _ready = true;

                        return;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_listenerThread.ThreadState == ThreadState.Unstarted)
            {
                return;
            }

            _shouldListen = false;

            _listenerThread.Join(TimeSpan.FromSeconds(10).Milliseconds);
        }

        public bool Ready()
        {
            return true;
        }

        private void Listen()
        {
            using (var connection = OpenConnection())
            {
                connection.Notification += HandleNotification;

                SendListenCommand(connection);

                while (_shouldListen)
                {
                    connection.Wait(TimeSpan.FromSeconds(1));
                }
            }
        }

        private void HandleNotification(object sender, NpgsqlNotificationEventArgs arguments)
        {
            var serializer = _memstateSettings.CreateSerializer();

            try
            {
                var row = JsonConvert.DeserializeObject<Row>(arguments.AdditionalInformation);

                var command = (Command) serializer.Deserialize(row.Command);

                var record = new JournalRecord(row.Id, row.Written, command);

                if (_ready)
                {
                    _handler(record);
                }
                else
                {
                    lock (Lock)
                    {
                        if (_ready)
                        {
                            _handler(record);
                        }
                        else
                        {
                            _buffer.Enqueue(record);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _log.LogError(exception, $"Exception occured in {GetType().Name}.HandleNotification");
            }
        }

        private void SendListenCommand(NpgsqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"LISTEN {_settings.SubscriptionStream};";

                command.ExecuteNonQuery();
            }
        }

        private NpgsqlConnection OpenConnection()
        {
            var connection = new NpgsqlConnection(_settings.ConnectionString);

            connection.Open();

            return connection;
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