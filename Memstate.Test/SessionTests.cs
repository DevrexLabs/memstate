namespace Memstate.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Memstate.Models;
    using Memstate.Models.KeyValue;
    using Memstate.Tcp;
    using Microsoft.CSharp.RuntimeBinder;
    using Xunit;

    public class SessionTests
    {
        private readonly Session<KeyValueStore<int>> _session;
        private readonly KeyValueStore<int> _testModel;
        private readonly List<Message> _messagesEmitted;

        public SessionTests()
        {
            var config = new Settings().WithInmemoryStorage();
            _testModel = new KeyValueStore<int>();
            var engine = new EngineBuilder(config).Build(_testModel);
            _session = new Session<KeyValueStore<int>>(config, engine);
            _messagesEmitted = new List<Message>();
            _session.OnMessage += _messagesEmitted.Add;
        }

        [Fact]
        public void Incompatible_command_emits_ExceptionResponse()
        {
            var command = new ProxyCommand<string>("dummy", null, null);
            var request = new CommandRequest(command);
            _session.Handle(request);
            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.Equal(request.Id, response.ResponseTo);
            Assert.IsType<InvalidCastException>(response.Exception);
        }

        [Fact]
        public void Query_that_throws_exception_emits_ExceptionResponse()
        {
            var query = new FailingQuery();
            var request = new QueryRequest(query);

            _session.Handle(request);

            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.Equal(request.Id, response.ResponseTo);
        }

        [Fact]
        public void Command_that_throws_exception_emits_ExceptionResponse()
        {
            var command = new Remove<int>("NON_EXISTING_KEY");
            var request = new CommandRequest(command);

            _session.Handle(request);

            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.Equal(request.Id, response.ResponseTo);
        }

        [Fact]
        public void QueryRequest_happy_path()
        {
            _testModel.Set("KEY", 42);
            var query = new Get<int>("KEY");
            var queryRequest = new QueryRequest(query);

            _session.Handle(queryRequest);

            Assert.Equal(1, _messagesEmitted.Count);
            var response = (QueryResponse)_messagesEmitted.Single();
            Assert.Equal(queryRequest.Id, response.ResponseTo);
            var node = (KeyValueStore<int>.Node)response.Result;
            Assert.Equal(42, node.Value);
        }

        [Fact]
        public void Command_with_result_happy_path()
        {
            var commandRequest = new CommandRequest(new Set<int>("KEY", 42));
            _session.Handle(commandRequest);
            var response = AssertAndGetSingle<CommandResponse>();
            Assert.Equal(commandRequest.Id, response.ResponseTo);
            Assert.Equal(1, (int)response.Result);
        }

        [Fact]
        public void Void_command_happy_path()
        {
            _testModel.Set("KEY", 100);
            var request = new CommandRequest(new Remove<int>("KEY"));

            _session.Handle(request);

            Assert.Equal(0, _testModel.Count());

            var response = AssertAndGetSingle<CommandResponse>();
            Assert.Equal(request.Id, response.ResponseTo);
            Assert.Null(response.Result);
        }

        [Fact]
        public void PingPong()
        {
            var request  = new Ping();
            _session.Handle(request);
            var pong = AssertAndGetSingle<Pong>();
            Assert.Equal(request.Id, pong.ResponseTo);
        }

        [Fact]
        public void UnhandledMessageException()
        {
            var message = new UnknownMessage();
            _session.Handle(message);

            var response = AssertAndGetSingle<ExceptionResponse>();
            Assert.IsType<RuntimeBinderException>(response.Exception);
            Assert.Equal(message.Id, response.ResponseTo);
        }

        private T AssertAndGetSingle<T>() where T : Message
        {
            Assert.Equal(1, _messagesEmitted.Count);
            var message = _messagesEmitted.Single();
            Assert.IsType<T>(message);
            return (T)message;
        }
    }

    internal class UnknownMessage : Message
    {

    }

    internal class FailingQuery : Query<KeyValueStore<int>, int>
    {
        public override int Execute(KeyValueStore<int> model)
        {
            throw new Exception("the innermost exception");
        }
    }
}