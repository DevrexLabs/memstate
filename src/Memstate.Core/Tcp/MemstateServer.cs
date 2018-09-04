using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Configuration;
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

        private Task _listenerTask;

        public MemstateServer(Engine<T> engine)
        {
            var settings = Config.Current.GetSettings<ServerSettings>();
            settings.Validate(); 
            _engine = engine;
            var ip = IPAddress.Parse(settings.Ip);
            var endPoint = new IPEndPoint(ip, settings.Port);
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
                var (ok, tcpClient) = await TryAcceptTcpClientAsync(_tcpListener);
                if (ok)
                {
                    _connections.Add(Task.Run(() => HandleConnection(tcpClient)));
                    _log.Info("Connection from {0}", tcpClient.Client.RemoteEndPoint);
                }
            }
        }

        private async Task<(bool, TcpClient)> TryAcceptTcpClientAsync(TcpListener listener)
        {
            try
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                return (true, tcpClient);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }
        private async Task HandleConnection(TcpClient tcpClient)
        {
            var serializer = Config.Current.CreateSerializer();
            var stream = tcpClient.GetStream();

            var outgoingMessages = new BlockingCollection<Message>();
            var writerTask = Task.Run(() => SendMessages(outgoingMessages, stream));
            var session = new Session<T>(_engine);
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
            var serializer = Config.Current.CreateSerializer();
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

        public async Task Stop()
        {
            _log.Info("Closing");
            _tcpListener.Stop();
            _cancellationSource.Cancel();
            await _listenerTask;
            _log.Debug("Closed");
        }
    }
}