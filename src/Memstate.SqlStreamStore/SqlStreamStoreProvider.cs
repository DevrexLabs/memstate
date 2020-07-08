using System;
using System.Text;
using System.Threading.Tasks;
using Memstate.Configuration;
using Npgsql;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreProvider: IStorageProvider
    {
        private readonly StreamId _streamId;
        private readonly ISerializer _serializer;
        private readonly IStreamStore _streamStore;

        private readonly bool UseSubscriptionBasedReader = false;
        private readonly Config _config;

        public SqlStreamStoreProvider(Config config) 
            : this(config, null) { }
        public SqlStreamStoreProvider(Config config, IStreamStore streamStore)
        {
            _config = config;
            _serializer = config.CreateSerializer();
            var settings = config.GetSettings<EngineSettings>();
            _streamId = new StreamId(settings.StreamName);

            if (streamStore == null)
            {
                if (!config.Container.TryResolve(out streamStore))
                    throw new Exception("Cannot resolve IStreamStore, call config.UseSqlStreamStore()");
            }

            _streamStore = streamStore;
        }

        public Task Provision() => Task.CompletedTask;

        /// <inheritdoc/>
        public IJournalReader CreateJournalReader()
        {
            return UseSubscriptionBasedReader ? (IJournalReader)
                new SqlStreamStoreSubscriptionJournalReader(
                    _streamStore,
                    _streamId,
                    _serializer) :
                new SqlStreamStoreJournalReader(
                    _streamStore,
                    _streamId,
                    _serializer);
        }

        /// <inheritdoc/>
        public IJournalWriter CreateJournalWriter()
        {
            return new SqlStreamStoreJournalWriter(_config, _streamStore, _streamId);
        }
        
        /// <summary>
        /// Initialize a postgres database for use as a Memstate SqlStreamStore backend
        /// <remarks>You must use this method to initialize the database objects.
        /// The normal SqlStreamStore schema uses the JSONB datatype</remarks>
        /// </summary>
        /// <param name="settings">A settings object with a valid connection string</param>
        public static void InitializePgSqlStreamStore(PostgresStreamStoreSettings settings)
        {
            var store = new PostgresStreamStore(settings);
            var originalScript = store.GetSchemaCreationScript();

            var sb = new StringBuilder("CREATE SCHEMA IF NOT EXISTS ");
            sb.AppendLine(settings.Schema + ";");
            
            sb.Append(originalScript);
            var script = sb.ToString().Replace("JSONB", "JSON");
            
            using (var connection = new NpgsqlConnection(settings.ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = script;
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}