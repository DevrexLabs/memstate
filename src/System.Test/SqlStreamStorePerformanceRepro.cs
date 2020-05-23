using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Memstate;
using Memstate.Configuration;
using Memstate.SqlStreamStore;
using Npgsql;
using NUnit.Framework;
using SqlStreamStore;

namespace System.Test
{
    [TestFixture, Ignore("Performance")]
    public class SqlStreamStorePerformanceRepro
    {

        private const int MessageSize = 400;
        private const int NumMessages = 10000;
        private const string PgsqlConnectionString
            = "Host=localhost; Password=postgres; User ID=postgres; Database=postgres;";

        private IJournalWriter _writer;
        private SqlStreamStoreProvider _provider;
        private String _streamName;

        private DbConnection _connection;
        private IStreamStore _streamStore;
        
        private readonly Stopwatch _stopWatch = new Stopwatch();

        [TearDown]
        public async Task TearDown()
        {
            await _connection.DisposeAsync();
            _streamStore.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            SqlStreamStore.Logging.LogProvider.IsDisabled = true;
            var config = Config.Current;
            _streamName = "test-" + DateTime.Now.ToFileTimeUtc(); 
            config.GetSettings<EngineSettings>().StreamName = _streamName;
            config.SerializerName = Serializers.NewtonsoftJson;
            
            //ConfigurePostgres();
            
            ConfigureMssql2019();
            _provider = new SqlStreamStoreProvider(_streamStore);
            _writer = _provider.CreateJournalWriter(0);
        }

        private void ConfigurePostgres()
        {
            _connection = new NpgsqlConnection(PgsqlConnectionString);
            var settings = new PostgresStreamStoreSettings(PgsqlConnectionString);
            settings.Schema = "randy";
            SqlStreamStoreProvider.InitializePgSqlStreamStore(settings);
            _streamStore = new PostgresStreamStore(settings);
        }
        

        private void ConfigureMssql2019()
        {
            CreateMsSqlDatabaseUnlessExists();
            var connectionString = "Server=localhost;Database=memstate;User Id=sa;Password=abc123ABC;";
            _connection = new SqlConnection(connectionString);
            var settings = new MsSqlStreamStoreV3Settings(connectionString);
            settings.Schema = "memstate";
            var store = new MsSqlStreamStoreV3(settings);
            store.CreateSchemaIfNotExists().GetAwaiter().GetResult();
            _streamStore = store;           
        }

        private void CreateMsSqlDatabaseUnlessExists()
        {
            try
            {
                var connectionString = "Server=localhost;Database=master;User Id=sa;Password=abc123ABC;";
                _connection = new SqlConnection(connectionString);
                _connection.Open();
                var cmd = _connection.CreateCommand();
                cmd.CommandText = "IF db_id('memstate') IS NULL CREATE DATABASE memstate";
                cmd.ExecuteNonQuery();
            }
            finally
            {
                _connection.Close();    
            }
        }

        [Test]
        public void DeserializationFailsWhenFirstJsonAttributeIsNotType()
        {
            Assert.Catch(() =>
            {
                var json =
                    "{\"Id\": \"8d7693f9-9bfc-4c23-ac52-31fc22bd67f3\", \"$type\": \"System.Test.SqlStreamStorePerformanceRepro+MyCommand, System.Test\", \"Payload\": {\"$type\": \"System.Byte[], System.Private.CoreLib\", \"$value\": \"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==\"}}";
                var serializer = Config.Current.CreateSerializer();
                var command = (Command) serializer.FromString(json);
            });
        }

        [Test]
        public void CanDeserializeWithTypeFirst()
        {
            var json = "{\"$type\":\"System.Test.SqlStreamStorePerformanceRepro+MyCommand, System.Test\",\"Payload\":{\"$type\":\"System.Byte[], System.Private.CoreLib\",\"$value\":\"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==\"},\"Id\":\"e12c9b93-025d-4378-b370-24704acd134d\"}";;
            var serializer = Config.Current.CreateSerializer();
            var command = (Command) serializer.FromString(json);
        }
        [Test]
        public async Task WriteTruncateAndLoadUsingReader()
        {
            await WriteAndTruncateMessages();
            _stopWatch.Restart();
            var reader = _provider.CreateJournalReader();
            var records = reader.GetRecords().ToList();
            Console.WriteLine("Records read: " + records.Count);
            Console.WriteLine("Read duration: " + _stopWatch.Elapsed);
            await reader.DisposeAsync();
        }

        [Test]
        public async Task WriteTruncateAndLoadUsingSubscription()
        {
            var messagesReceived = 0;
            
            await WriteAndTruncateMessages();
            _stopWatch.Restart();
            var sub = _provider.CreateJournalSubscriptionSource()
                .Subscribe(0, jr => messagesReceived++);
            while (!sub.Ready()) await Task.Delay(TimeSpan.FromMilliseconds(20));
            Console.WriteLine("Messages received: " + messagesReceived);
            Console.WriteLine("Read with sub duration: " + _stopWatch.Elapsed);
            sub.Dispose();
        }

        private async Task WriteAndTruncateMessages()
        {
            _stopWatch.Restart();
            await AppendMessages(NumMessages, MessageSize);
            Console.WriteLine("Append duration:" + _stopWatch.Elapsed);
            _stopWatch.Restart();
            //await DeleteMessages();
        }
        private async Task DeleteMessages()
        {
            await _connection.OpenAsync();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "TRUNCATE TABLE messages";
            cmd.CommandTimeout = 1000 * 60 * 5; //no idea if this is 5 minutes or 5000 minutes
            var result = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Messages deleted: " + result);
        }

        private async Task AppendMessages(int numMessages, int messageSize)
        {
            for (int i = 0; i < numMessages; i++)
            {
                _writer.Send(new MyCommand(messageSize));
            }
            await _writer.DisposeAsync();
            Console.WriteLine("Append complete: " + DateTime.Now);
        }

        public class MyCommand : Command
        {
            public byte[] Payload { get; set; }

            public MyCommand(int size)
            {
                Payload = new byte[size];
            }

            internal override object ExecuteImpl(object model)
            {
                //never called
                throw new NotImplementedException();
            }
        }
    }
}