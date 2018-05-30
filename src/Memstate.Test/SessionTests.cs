using NUnit.Framework;

namespace Memstate.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Memstate.Models;
    using Memstate.Models.KeyValue;
    using Memstate.Tcp;

    public class SessionTests
    {
        private  Session<KeyValueStore<int>> _session;
        private KeyValueStore<int> _testModel;
        private List<Message> _messagesEmitted;

        [SetUp]
        public void PerTestSetup()
        {
            var config = new MemstateSettings().WithInmemoryStorage();
            _testModel = new KeyValueStore<int>();
            var engine = new EngineBuilder(config).Build(_testModel);
            _session = new Session<KeyValueStore<int>>(config, engine);
            _messagesEmitted = new List<Message>();
            _session.OnMessage += _messagesEmitted.Add;
        }

        [Test]
        public void Incompatible_command_emits_ExceptionResponse()
        {
            var command = new ProxyCommand<string>("dummy", null, null);
            var request = new CommandRequest(command);
            _session.Handle(request);
            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.AreEqual(request.Id, response.ResponseTo);
            Assert.IsAssignableFrom<InvalidCastException>(response.Exception);
        }

        [Test]
        public void Query_that_throws_exception_emits_ExceptionResponse()
        {
            var query = new FailingQuery();
            var request = new QueryRequest(query);

            _session.Handle(request);

            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.AreEqual(request.Id, response.ResponseTo);
        }

        [Test]
        public void Command_that_throws_exception_emits_ExceptionResponse()
        {
            var command = new Remove<int>("NON_EXISTING_KEY");
            var request = new CommandRequest(command);

            _session.Handle(request);

            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.AreEqual(request.Id, response.ResponseTo);
        }

        [Test]
        public void QueryRequest_happy_path()
        {
            _testModel.Set("KEY", 42);
            var query = new Get<int>("KEY");
            var queryRequest = new QueryRequest(query);

            _session.Handle(queryRequest);

            Assert.IsTrue(_messagesEmitted.Count == 1);
            var response = (QueryResponse)_messagesEmitted.Single();
            Assert.AreEqual(queryRequest.Id, response.ResponseTo);
            var node = (KeyValueStore<int>.Node)response.Result;
            Assert.AreEqual(42, node.Value);
        }

        [Test]
        public void Command_with_result_happy_path()
        {
            var commandRequest = new CommandRequest(new Set<int>("KEY", 42));
            _session.Handle(commandRequest);
            var response = AssertAndGetSingle<CommandResponse>();
            Assert.AreEqual(commandRequest.Id, response.ResponseTo);
            Assert.AreEqual(1, (int)response.Result);
        }

        [Test]
        public void Void_command_happy_path()
        {
            _testModel.Set("KEY", 100);
            var request = new CommandRequest(new Remove<int>("KEY"));

            _session.Handle(request);

            Assert.AreEqual(0, _testModel.Count());

            var response = AssertAndGetSingle<CommandResponse>();
            Assert.AreEqual(request.Id, response.ResponseTo);
            Assert.IsNull(response.Result);
        }

        [Test]
        public void PingPong()
        {
            var request  = new Ping();
            _session.Handle(request);
            var pong = AssertAndGetSingle<Pong>();
            Assert.AreEqual(request.Id, pong.ResponseTo);
        }

        [Test]
        public void UnhandledMessageException()
        {
            var message = new UnknownMessage();
            _session.Handle(message);

            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.IsAssignableFrom<Exception>(response.Exception);
            Assert.AreEqual(message.Id, response.ResponseTo);
        }

        private T AssertAndGetSingle<T>() where T : Message
        {
            Assert.IsTrue(_messagesEmitted.Count == 1);
            var message = _messagesEmitted.Single();
            Assert.IsAssignableFrom<T>(message);
            return (T)message;
        }

        internal class UnknownMessage : Message
        {
        }

        internal class FailingQuery : Query<KeyValueStore<int>, int>
        {
            public override int Execute(KeyValueStore<int> db)
            {
                throw new Exception("the innermost exception");
            }
        }
    }
}