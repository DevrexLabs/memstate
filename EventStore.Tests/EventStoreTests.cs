using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Memstate;
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

    public class EventStoreTests : MemstateTestBase, IDisposable
    {
        private readonly IEventStoreConnection _connection;
        private readonly EventStoreConnectionMonitor _monitor;
        private readonly string _streamName;

        public EventStoreTests(ITestOutputHelper log) : base(log)
        {
            const string cstr = "ConnectTo=tcp://admin:changeit@localhost:1113";
            _connection = EventStoreConnection.Create(cstr);
            _connection.ConnectAsync().Wait();
            _monitor = new EventStoreConnectionMonitor(Config, _connection);
            _streamName = "xunit-test-" + Guid.NewGuid();
        }

        public void Dispose()
        {
            _connection.Close();
        }

        [Fact]
        public void EnginePoc_command_round_trip()
        {
            var numCommands = 1000;
            var engine = new EnginePoc<List<string>>(_connection);
            for (int i = 0; i < numCommands; i++)
            {
                engine.ExecuteAsync(new AddStringCommand() {StringToAdd = i.ToString()});
            }
            engine.Dispose();
            Assert.Equal(numCommands, engine.CommandsReceived);

        }

        [Fact]
        public void CanWriteOne()
        {
            var serializer = new JsonSerializerAdapter();
            var eventStoreWriter = new EventStoreWriter(Config, _connection, serializer, _streamName);
            eventStoreWriter.Send(new AddStringCommand());
            eventStoreWriter.Dispose();
            var reader = new EventStoreReader(Config, _connection, serializer, _streamName);
            var records = reader.GetRecords().ToArray();
            reader.Dispose();
            Assert.Equal(1, records.Length);
        }

        [Fact]
        public void CanWriteMany()
        {
            var serializer = new JsonSerializerAdapter();
            var eventStoreWriter = new EventStoreWriter(Config, _connection, serializer, _streamName);
            for(var i = 0; i < 10000; i++)
            {
                eventStoreWriter.Send(new AddStringCommand());
            }
            
            eventStoreWriter.Dispose();
            var reader = new EventStoreReader(Config, _connection, serializer, _streamName);
            var records = reader.GetRecords().ToArray();
            reader.Dispose();
            Assert.Equal(10000, records.Length);
        }

        [Fact]
        public void SubscriptionFiresEventAppeared()
        {
            const int numRecords = 50;
            var serializer = new JsonSerializerAdapter();
            var eventStoreWriter = new EventStoreWriter(Config, _connection, serializer, _streamName);
            for (var i = 0; i < numRecords; i++)
            {
                eventStoreWriter.Send(new AddStringCommand());
            }
            eventStoreWriter.Dispose();

            var records = new List<JournalRecord>();
            var subSource = new EventStoreSubscriptionSource(Config, _connection, serializer, _streamName);
            var sub = (EventStoreSubscriptionAdapter) subSource.Subscribe(0, records.Add);
            while (!sub.Ready()) Thread.Sleep(0);
            Assert.True(
                records.Select(r => (int)r.RecordNumber)
                .SequenceEqual(Enumerable.Range(0,numRecords)));

        }

        [Fact]
        public void EventsBatchWrittenAppearOnCatchUpSubscription()
        {
            const int numRecords = 5;

            //arrange
            var serializer = new JsonSerializerAdapter();
            var records = new List<JournalRecord>();
            var sub = new EventStoreSubscriptionSource(Config, _connection, serializer, _streamName).Subscribe(0, records.Add);
            var writer = new EventStoreWriter(Config, _connection, serializer, _streamName);

            //act
            for (int i = 0; i < numRecords; i++)
            {
                writer.Send(new AddStringCommand());
            }
            writer.Dispose();
            while(records.Count < 5) Thread.Sleep(0);
            sub.Dispose();

            Assert.Equal(5, records.Count);
        }

        [Fact]
        public void EventsWrittenAppearOnCatchUpSubscription()
        {
            //arrange
            var serializer = new JsonSerializerAdapter();
            var records = new List<JournalRecord>();
            var sub = new EventStoreSubscriptionSource(Config, _connection, serializer, _streamName).Subscribe(0, records.Add);
            var writer = new EventStoreWriter(Config, _connection,serializer, _streamName);

            //act
            writer.Send(new AddStringCommand());
            writer.Send(new AddStringCommand());
            writer.Send(new AddStringCommand());
            writer.Send(new AddStringCommand());
            writer.Send(new AddStringCommand());
            writer.Dispose();
            while (!sub.Ready()) Thread.Sleep(0);
            sub.Dispose();

            Assert.Equal(5, records.Count);
        }

        public class Reverse : Command<List<String>>
        {
            public override void Execute(List<string> model)
            {
                model.Reverse();
            }
        }

        [Fact]
        public void Can_execute_void_commands()
        {
            var serializer = new JsonSerializerAdapter();
            var builder = new EventStoreEngineBuilder(Config, _connection, serializer, _streamName);
            Engine<List<string>> engine = builder.Build<List<string>>();

            engine.ExecuteAsync(new Reverse());
            engine.Dispose();
        }

        [Fact]
        public void Smoke()
        {
            const int numRecords = 1;
            var serializer = new JsonSerializerAdapter();
            var builder = new EventStoreEngineBuilder(Config, _connection, serializer, _streamName);
            Engine<List<string>> engine = builder.Build<List<string>>();

            var tasks = Enumerable.Range(10, numRecords)
                .Select(n => engine.ExecuteAsync(new AddStringCommand(){StringToAdd = n.ToString()}))
                .ToArray();
            //Task.WaitAll(tasks);
            int expected = 1;
            foreach (var task in tasks) Assert.Equal(expected++, task.Result);
            //foreach (var number in Enumerable.Range(1,100))
            //{
            //    var command = new AddStringCommand() {StringToAdd = number.ToString()};
            //    var count = await engine.ExecuteAsync(command);
            //    _log.WriteLine("executed " + number);
            //    Assert.Equal(number, count);
            //}

            engine.Dispose();

            //is the builder reusable?
            //can we load when there are existing commands in the stream
            engine = builder.Build<List<string>>();
            var strings = engine.Execute(new GetStringsQuery());
            Assert.Equal(numRecords, strings.Count);
            engine.Dispose();
        }
    }
}
