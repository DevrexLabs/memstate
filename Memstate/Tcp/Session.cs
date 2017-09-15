using System;
using Memstate;
using Microsoft.Extensions.Logging;

namespace Memstate.Tcp
{
    /// <summary>
    /// Processes and reacts to messages from
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Session<T> : IHandle<NetworkMessage> where T : class
    {
        private readonly Engine<T> _engine;
        private readonly ILogger _logger;

        public Session(Config config, Engine<T> engine)
        {
            _engine = engine;
            _logger = config.LoggerFactory.CreateLogger<Session<T>>();
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
            try
            {
                dynamic d = message;
                Handle(d);
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogError(_logger, ex, "Failed to handle message of type " + message.GetType());
                throw;
            }
        }
    }
}