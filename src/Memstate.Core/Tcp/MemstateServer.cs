using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Logging;

namespace Memstate.Tcp
{
    /// <summary>
    /// TCP Server implementation.
    /// Listens for tcp connections and spins off a <c>Session</c> for each incoming request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MemstateServer<T> where T : class
    {
        private readonly Engine<T> _engine;

        private readonly TcpListener _tcpListener;

        private readonly ILog _log;

        private readonly CancellationTokenSource _cancellationSource;

        private readonly ISet<Task> _connections = new HashSet<Task>();

        private readonly MemstateSettings _config;

        private Task _listenerTask;

        public MemstateServer(MemstateSettings config, Engine<T> engine)
        {
            _engine = engine;
            _config = config;

            // TODO: Endpoint should be configurable.
            var ip = IPAddress.Any;
            var endPoint = new IPEndPoint(ip, 3001);
            _tcpListener = new TcpListener(endPoint);
            _log = LogProvider.GetCurrentClassLogger();
            _cancellationSource = new CancellationTokenSource();
        }

        public void Start()
        {
            _log.Info("Starting");
            _tcpListener.Start();
            _listenerTask = Task.Run(() => AcceptConnections(_cancellationSource.Token));
            _log.Debug("Started");
        }

        private async Task AcceptConnections(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                _connections.Add(Task.Run(() => HandleConnection(tcpClient)));
                _log.Info("Connection from {0}", tcpClient.Client.RemoteEndPoint);
            }
        }

        private async Task HandleConnection(TcpClient tcpClient)
        {
            var serializer = _config.CreateSerializer();
            var stream = tcpClient.GetStream();

            var outgoingMessages = new BlockingCollection<Message>();
            var writerTask = Task.Run(() => SendMessages(outgoingMessages, stream));
            var session = new Session<T>(_config, _engine);
            session.OnMessage += outgoingMessages.Add;

            while (!_cancellationSource.Token.IsCancellationRequested)
            {
                _log.Debug("Waiting for message");
                var message = await Message.Read(stream, serializer, _cancellationSource.Token);
                _log.Debug("Received {0} from {1}", message, tcpClient.Client.RemoteEndPoint);
                await session.Handle(message);
            }

            outgoingMessages.CompleteAdding();
            await writerTask;
        }

        private async Task SendMessages(BlockingCollection<Message> messages, Stream stream)
        {
            // TODO: Consider using MessageProcessor.
            var serializer = _config.CreateSerializer();
            var cancellationToken = _cancellationSource.Token;
            var messageId = 0;

            while (!messages.IsCompleted)
            {
                var message = messages.TakeOrDefault(cancellationToken);

                if (message == null) break;

                var bytes = serializer.Serialize(message);
                var packet = Packet.Create(bytes, ++messageId);
                await packet.WriteTo(stream);
                await stream.FlushAsync();
            }
        }

        public void Stop()
        {
            _log.Info("Closing");
            _cancellationSource.Cancel();
            _listenerTask.Wait();
            // TODO: Should we stop before Cancel() and Wait() ?
            _tcpListener.Stop();
            _log.Debug("Closed");
        }
    }
}