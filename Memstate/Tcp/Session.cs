using System;
using Microsoft.Extensions.Logging;

namespace Memstate.Tcp
{
    /// <summary>
    /// Handles incoming messages and emits outgoing messages 
    /// for a given client connection.
    /// </summary>
    internal class Session<T> : IHandle<Message> where T : class
    {
        // TODO: Implement the event handler.
        private readonly Action<Event> _remoteEventHandler = e => { };
        
        private readonly Engine<T> _engine;
        private readonly ILogger _logger;

        public Session(MemstateSettings config, Engine<T> engine)
        {
            _engine = engine;
            _logger = config.LoggerFactory.CreateLogger<Session<T>>();
        }

        public event Action<Message> OnMessage = _ => { };

        private void HandleImpl(QueryRequest request)
        {
            var result = _engine.Execute(request.Query);
            var response = new QueryResponse(result, request.Id);
            OnMessage.Invoke(response);
        }

        private void HandleImpl(CommandRequest request)
        {
            var result = _engine.Execute(request.Command, _remoteEventHandler);
            var response = new CommandResponse(result, request.Id);
            OnMessage.Invoke(response);
        }

        private void HandleImpl(Ping ping)
        { 
            OnMessage.Invoke(new Pong(ping));
        }

        public void Handle(Message message)
        {
            try
            {
                dynamic d = message;
                HandleImpl(d);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message: " + message);
                OnMessage.Invoke(new ExceptionResponse(message, ex));
            }
        }
    }
}