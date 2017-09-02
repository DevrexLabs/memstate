using System;
using System.Reflection;

namespace Memstate.Tcp
{

    internal class ServerProtocol<T> :
        IHandle<NetworkMessage>,
        IHandle<QueryRequest>,
        IHandle<CommandRequest>,
        IHandle<Ping>
        where T : class
    {

        private readonly Engine<T> _engine;

        public ServerProtocol(Engine<T> engine)
        {
            _engine = engine;
        }


        public event Action<NetworkMessage> OnMessage = _ => { };

        public void Handle(QueryRequest message)
        {
            var result = _engine.Execute(message.Query);
            var response = new QueryResponse
            {
                Result = result
            };
            OnMessage.Invoke(response);
        }

        public void Handle(CommandRequest message)
        {
        }

        public void Handle(Ping message)
        {
            OnMessage.Invoke(new Pong(message.Id));
        }

        public void Handle(NetworkMessage message)
        {
            //using reflection. couldn't get dynamic working and an attempt at generics turned into a mess
            //todo: redesign or at least cache
            var methodInfo =  GetType().GetRuntimeMethod("Handle", new[] {message.GetType()});
            methodInfo.Invoke(this, new[] {message});
        }
    }

    internal class QueryResponse : NetworkMessage
    {
        public object Result { get; set; }
    }
}
