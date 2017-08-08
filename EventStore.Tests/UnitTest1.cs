using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Memstate.Core;
using Memstate.EventStore;
using Memstate.JsonNet;
using Xunit;
using Xunit.Abstractions;

namespace EventStore.Tests
{
    public class AddStringCommand : Command<List<string>, int>
    {
        public String StringToAdd { get; set; }

        public override int Execute(List<string> model)
        {
            model.Add(StringToAdd);
            return model.Count;
        }
    }

    public class GetStringsQuery : Query<List<string>, List<string>>
    {
        public override List<string> Execute(List<string> model)
        {
            return model;
        }
    }

    public class UnitTest1
    {
        private readonly ITestOutputHelper _log;

        public UnitTest1(ITestOutputHelper log)
        {
            _log = log;
        }

        [Fact]
        public async Task CanWrite()
        {
            var streamName = "xunit-test-" + Guid.NewGuid();
            var cstr = "ConnectTo=tcp://admin:changeit@localhost:1113; VerboseLogging=True";
            var connection = EventStoreConnection.Create(cstr);
            await connection.ConnectAsync();
            var serializer = new JsonSerializerAdapter();

            var eventStoreWriter = new EventStoreWriter(connection, serializer, streamName);
            eventStoreWriter.AppendAsync(new AddStringCommand());
            eventStoreWriter.Dispose();
            var reader = new EventStoreReader(connection, serializer, streamName);
            var records = reader.GetRecords().ToArray();
            Assert.Equal(1, records.Length);
            
            reader.Dispose();
            connection.Close();
        }

        [Fact]
        public async Task Smoke()
        {
            var streamName = "xunit-test-" + Guid.NewGuid();
            var cstr = "ConnectTo=tcp://admin:changeit@localhost:1113; VerboseLogging=True";
            var connection = EventStoreConnection.Create(cstr);
            await connection.ConnectAsync();
            var serializer = new JsonSerializerAdapter();
            var builder = new EventStoreEngineBuilder(connection, serializer, streamName);
            Engine<List<string>> engine = builder.Load<List<string>>();
            _log.WriteLine("engine loaded");
            foreach (var number in Enumerable.Range(1,100))
            {
                var command = new AddStringCommand() {StringToAdd = number.ToString()};
                var count = await engine.ExecuteAsync(command);
                _log.WriteLine("executed " + number);
                Assert.Equal(number, count);
            }
            engine.Dispose();
            _log.WriteLine("dispose after write, loading...");

            //is the builder reusable?
            //can we load when there are existing commands in the stream
            engine = builder.Load<List<string>>();
            var strings = engine.Execute(new GetStringsQuery());
            Assert.Equal(100, strings.Count);
            engine.Dispose();
            connection.Close();
        }
    }
}
