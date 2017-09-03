using System;
using System.Reflection;

namespace Memstate.Tcp
{
    internal class ServerProtocol<T> : IHandle<NetworkMessage> where T : class
    {
        private readonly Engine<T> _engine;

        public ServerProtocol(Engine<T> engine)
        {
            _engine = engine;
        }

        public event Action<NetworkMessage> OnMessage = _ => { };

        public void Handle(QueryRequest request)
        {
            var result = _engine.Execute(request.Query);
            var response = new QueryResponse(result, request.Id);
            OnMessage.Invoke(response);
        }

        private void Handle(CommandRequest message)
        {
        }

        private void Handle(Ping message)
        {
            OnMessage.Invoke(new Pong(message.Id));
        }

        public void Handle(NetworkMessage message)
        {
            //using reflection. couldn't get dynamic working and an attempt at generics turned into a mess
            //todo: redesign or at least cache
            var methodInfo =  GetType().GetRuntimeMethod("Handle", new[] {message.GetType()});
            methodInfo.Invoke(this, new object[] {message});
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
