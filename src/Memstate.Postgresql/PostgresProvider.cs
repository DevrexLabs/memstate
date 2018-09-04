﻿using Memstate.Logging;
using System.Threading.Tasks;
using Npgsql;
using Memstate.Configuration;
using Npgsql.Logging;
using System.Diagnostics;

namespace Memstate.Postgres
{
    public class PostgresProvider : StorageProvider
    {
        private readonly ILog _log;
        private bool _initialized;

        public PostgresProvider()
        {
            _log = LogProvider.GetCurrentClassLogger();
            Settings = Config.Current.GetSettings<PostgresSettings>();
        }

        static PostgresProvider()
        {
            EnableNpgsqlDebugLogging();
        }

        [Conditional("PGTRACE")]
        private static void EnableNpgsqlDebugLogging()
        {
            NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Trace, true, true);
            NpgsqlLogManager.IsParameterLoggingEnabled = true;
        }

        public PostgresSettings Settings { get; }

        public override void Initialize()
        {
            if (_initialized) return;
            _log.Debug("Initializing...");

            var sql = Settings.InitSql.Value;
            
            using (var connection = new NpgsqlConnection(Settings.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = string.Format(sql, Settings.SubscriptionStream, Settings.Table);
                command.ExecuteNonQuery();
            }
            _initialized = true;
        }

        public override IJournalReader CreateJournalReader()
        {
            return new PostgresJournalReader(Settings);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            // todo: nextRecordNumber unused
            var serializer = Config.Current.CreateSerializer();
            return new PostgresJournalWriter(serializer, Settings);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new PostgresSubscriptionSource(Settings);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}