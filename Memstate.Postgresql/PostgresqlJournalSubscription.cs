using System;
using System.Threading;
using Npgsql;

namespace Memstate.Postgresql
{
    public class PostgresqlJournalSubscription : IJournalSubscription
    {
        private readonly AutoResetEvent _readWaiter = new AutoResetEvent(false);
        private readonly Thread _listenerThread;
        private readonly Thread _readerThread;
        private readonly PostgresqlSettings _settings;
        private readonly Action<JournalRecord> _handler;
        private readonly PostgresqlJournalReader _journalReader;
        private bool _ready;
        private volatile bool _disposed;

        /// <summary>
        /// Id of the next record to read
        /// </summary>
        private long _nextRecordId;

        public PostgresqlJournalSubscription(PostgresqlSettings settings, Action<JournalRecord> handler, long nextRecordId)
        {
            Ensure.NotNull(settings, nameof(settings));

            _settings = settings;
            _handler = handler;
            _nextRecordId = nextRecordId;

            _journalReader = new PostgresqlJournalReader(settings);

            _listenerThread = new Thread(Listen)
            {
                Name = "Memstate:Postgresql:NotificationsListener"
            };

            _readerThread = new Thread(Reader)
            {
                Name = "Memstate:Postgresql:Reader"
            };
        }

        public PostgresqlJournalSubscription(MemstateSettings settings, Action<JournalRecord> handler, long nextRecordId)
            : this(new PostgresqlSettings(settings), handler, nextRecordId)
        {
        }

        public void Start()
        {
            _readerThread.Start();
            _listenerThread.Start();

            while (!Ready())
            {
                Thread.Sleep(0);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _readWaiter.Set();
            _readerThread.Join(TimeSpan.FromSeconds(10).Milliseconds);
            _listenerThread.Join(TimeSpan.FromSeconds(10).Milliseconds);
        }

        public bool Ready()
        {
            return _ready;
        }

        private void Listen()
        {
            using (var connection = OpenConnection())
            {
                connection.Notification += HandleNotification;

                SendListenCommand(connection);
                  
                while (!_disposed)
                {
                    connection.Wait(TimeSpan.FromSeconds(10));
                }
            }
        }

        private void Reader()
        {
            while (!_disposed)
            {
                foreach (var record in _journalReader.GetRecords(_nextRecordId))
                {
                    _nextRecordId++;
                    _handler.Invoke(record);
                }

                _ready = true;
                _readWaiter.WaitOne(TimeSpan.FromSeconds(1));
            }
        }

        private void HandleNotification(object sender, NpgsqlNotificationEventArgs arguments)
        {
            _readWaiter.Set();
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
    }
}