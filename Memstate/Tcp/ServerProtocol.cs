using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Memstate.Tcp
{
    internal class ServerProtocol<T> : IHandle<NetworkMessage> where T : class
    {
        private readonly Engine<T> _engine;
        private readonly ILogger _logger;

        public ServerProtocol(Config config, Engine<T> engine)
        {
            _engine = engine;
            _logger = config.LoggerFactory.CreateLogger<ServerProtocol<T>>();
        }

        public event Action<NetworkMessage> OnMessage = _ => { };

        public void Handle(QueryRequest request)
        {
            var result = _engine.Execute(request.Query);
            var response = new QueryResponse(result, request.Id);
            OnMessage.Invoke(response);
        }

        private void Handle(CommandRequest request)
        {
            var result = _engine.Execute(request.Command);
            var response = new CommandResponse(result, request.Id);
            OnMessage.Invoke(response);
        }

        private void Handle(Ping message)
        {
            OnMessage.Invoke(new Pong(message.Id));
        }

        public void Handle(NetworkMessage message)
        {
            if (message is CommandRequest commandRequest) Handle(commandRequest);
            else if (message is QueryRequest queryRequest) Handle(queryRequest);
            else if (message is Ping ping) Handle(ping);
            else _logger.LogError("Ignoring unrecognized message: " + message);
        }
    }

    internal class Response : NetworkMessage
    {
        public Response(Guid responseTo)
        {
            ResponseTo = responseTo;
        }

        public Guid ResponseTo { get; }
    }

    internal class QueryResponse : Response
    {
        public QueryResponse(object result, Guid responseTo)
            : base(responseTo)
        {
            Result = result;
        }

        public object Result { get; }
    }
}
