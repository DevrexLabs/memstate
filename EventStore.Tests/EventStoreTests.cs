using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    public class EventStoreTests
    {
        private readonly ITestOutputHelper _log;

        public EventStoreTests(ITestOutputHelper log)
        {
            _log = log;
            TestOutputLoggingProvider.SetOutputHelper(log);
        }

        [Fact]
        public async Task CanWriteOne()
        {
            var streamName = "xunit-test-" + Guid.NewGuid();
            var cstr = "ConnectTo=tcp://admin:changeit@localhost:1113; VerboseLogging=True";
            var connection = EventStoreConnection.Create(cstr);
            await connection.ConnectAsync();
            var serializer = new JsonSerializerAdapter();

            var eventStoreWriter = new EventStoreWriter(connection, serializer, streamName);
            eventStoreWriter.Send(new AddStringCommand());
            eventStoreWriter.Dispose();
            var reader = new EventStoreReader(connection, serializer, streamName);
            var records = reader.GetRecords().ToArray();
            reader.Dispose();
            connection.Close();
            _log.WriteLine("hello");
            Assert.Equal(1, records.Length);
        }

        [Fact]
        public async Task CanWriteMany()
        {
            var streamName = "xunit-test-" + Guid.NewGuid();
            var cstr = "ConnectTo=tcp://admin:changeit@localhost:1113; VerboseLogging=True";
            var connection = EventStoreConnection.Create(cstr);
            await connection.ConnectAsync();
            var serializer = new JsonSerializerAdapter();

            var eventStoreWriter = new EventStoreWriter(connection, serializer, streamName);
            for(var i = 0; i < 10000; i++)
            {
                eventStoreWriter.Send(new AddStringCommand());
            }
            
            eventStoreWriter.Dispose();
            var reader = new EventStoreReader(connection, serializer, streamName);
            var records = reader.GetRecords().ToArray();
            reader.Dispose();
            connection.Close();
            _log.WriteLine("hello");
            Assert.Equal(10000, records.Length);
        }

        [Fact]
        public async Task SubscriptionFiresEventAppeared()
        {
            const int numRecords = 50;
            var streamName = "xunit-test-" + Guid.NewGuid();
            var cstr = "ConnectTo=tcp://admin:changeit@localhost:1113; VerboseLogging=True";
            var connection = EventStoreConnection.Create(cstr);
            await connection.ConnectAsync();
            var serializer = new JsonSerializerAdapter();
            var eventStoreWriter = new EventStoreWriter(connection, serializer, streamName);
            for (var i = 0; i < numRecords; i++)
            {
                eventStoreWriter.Send(new AddStringCommand());
            }
            eventStoreWriter.Dispose();

            var records = new List<JournalRecord>();
            var subSource = new EventStoreSubscriptionSource(connection, serializer, streamName);
            var sub = (EventStoreSubscriptionAdapter) subSource.Subscribe(0, records.Add);
            while (!sub.Ready()) Thread.Sleep(0);
            connection.Close();
            Assert.True(
                records.Select(r => (int)r.RecordNumber)
                .SequenceEqual(Enumerable.Range(0,numRecords)));

        }

        [Fact]
        public async Task EventsBatchWrittenAppearOnCatchUpSubscription()
        {
            //arrange
            var streamName = "xunit-test-" + Guid.NewGuid();
            var cstr = "ConnectTo=tcp://admin:changeit@localhost:1113; VerboseLogging=True";
            var connection = EventStoreConnection.Create(cstr);
            await connection.ConnectAsync();
            var serializer = new JsonSerializerAdapter();
            var records = new List<JournalRecord>();
            var sub = new EventStoreSubscriptionSource(connection, serializer, streamName).Subscribe(0, records.Add);
            var writer = new EventStoreWriter(connection, serializer, streamName);

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

        [Fact]
        public async Task EventsWrittenAppearOnCatchUpSubscription()
        {
            //arrange
            var streamName = "xunit-test-" + Guid.NewGuid();
            var cstr = "ConnectTo=tcp://admin:changeit@localhost:1113; VerboseLogging=True";
            var connection = EventStoreConnection.Create(cstr);
            await connection.ConnectAsync();
            var serializer = new JsonSerializerAdapter();
            var records = new List<JournalRecord>();
            var sub = new EventStoreSubscriptionSource(connection, serializer, streamName).Subscribe(0, records.Add);
            var writer = new EventStoreWriter(connection,serializer, streamName);

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


        [Fact]
        public async Task Smoke()
        {
            const int numRecords = 1;
            var streamName = "xunit-test-" + Guid.NewGuid();
            var cstr = "ConnectTo=tcp://admin:changeit@localhost:1113; VerboseLogging=True";
            var connection = EventStoreConnection.Create(cstr);
            await connection.ConnectAsync();
            var serializer = new JsonSerializerAdapter();
            var builder = new EventStoreEngineBuilder(connection, serializer, streamName);
            Engine<List<string>> engine = builder.Load<List<string>>();

            var tasks = Enumerable.Range(10, numRecords)
                .Select(n => engine.ExecuteAsync(new AddStringCommand(){StringToAdd = n.ToString()}))
                .ToArray();
            Task.WaitAll(tasks);
            //int expected = 1;
            //foreach (var task in tasks) Assert.Equal(expected++, task.Result);
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
            engine = builder.Load<List<string>>();
            var strings = engine.Execute(new GetStringsQuery());
            Assert.Equal(numRecords, strings.Count);
            engine.Dispose();
            connection.Close();
        }
    }
}
