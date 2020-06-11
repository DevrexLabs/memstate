using System.Linq;
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

        private const int MessageSize = 1024;
        private const int NumMessages = 200000;
        private const string PgsqlConnectionString
            = "Host=localhost; Password=postgres; User ID=postgres; Database=postgres;";

        private IJournalWriter _writer;
        private SqlStreamStoreProvider _provider;
        private String _streamName;


        [SetUp]
        public void Setup()
        {
            var config = Config.Current;
            _streamName = "test-" + DateTime.Now.ToFileTimeUtc(); 
            config.GetSettings<EngineSettings>().StreamName = _streamName;
                

            config.SerializerName = Serializers.NewtonsoftJson;
            var streamStore = Postgres();
            _provider = new SqlStreamStoreProvider(streamStore);
            _writer = _provider.CreateJournalWriter(0);

        }

        [Test]
        public async Task WriteTruncateAndLoadUsingReader()
        {
            Console.WriteLine(nameof(WriteTruncateAndLoadUsingReader) + " " + _streamName);
            await AppendMessages(NumMessages, MessageSize);
            await DeleteMessages();
            var reader = _provider.CreateJournalReader();
            var records = reader.GetRecords().ToList();
            Console.WriteLine("Records read: " + records.Count);
            Console.WriteLine(DateTime.Now);
        }

        [Test]
        public async Task WriteTruncateAndLoadUsingSubscription()
        {
            Console.WriteLine(nameof(WriteTruncateAndLoadUsingSubscription) + " " + _streamName);
            await AppendMessages(NumMessages, MessageSize);
            await DeleteMessages();
            var sub = _provider.CreateJournalSubscriptionSource()
                .Subscribe(0, jr => Console.WriteLine(jr.RecordNumber));
            while (!sub.Ready()) await Task.Delay(TimeSpan.FromMilliseconds(20));
            Console.WriteLine("Ready! " + DateTime.Now);
        }

        private async Task DeleteMessages()
        {
            var connection = new NpgsqlConnection(PgsqlConnectionString);
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM messages";
            var result = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Messages deleted: " + result);

        }
        private IStreamStore Postgres()
        {
            var settings = new PostgresStreamStoreSettings(PgsqlConnectionString);
            var store = new PostgresStreamStore(settings);
            store.CreateSchemaIfNotExists().GetAwaiter().GetResult();
            return store;
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

        private class MyCommand : Command
        {
            public byte[] Payload { get; set; }

            public MyCommand(int size)
            {
                Payload = new byte[size];
            }

            internal override object ExecuteImpl(object model)
            {
                throw new NotImplementedException();
            }
        }
    }
}