using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Memstate.Tcp
{
    public class MemstateServer<T> where T: class
    {
        private readonly Engine<T> _engine;
        private readonly TcpListener _tcpListener;
        private Task _listenerTask;
        private readonly ILogger _log;
        private readonly CancellationTokenSource _cancellationSource;
        private readonly ISet<Task> _connections = new HashSet<Task>();
        private readonly Config _config;

        public MemstateServer(Config config, Engine<T> engine)
        {
            _engine = engine;
            _config = config;
            var ip = IPAddress.Parse("127.0.0.1");
            var endPoint = new IPEndPoint(ip, 3001);
            _tcpListener = new TcpListener(endPoint);
            _log = config.LoggerFactory.CreateLogger(typeof(MemstateServer<>));
            _cancellationSource = new CancellationTokenSource();
        }

        public void Start()
        {
            _log.LogInformation("Starting");
            _tcpListener.Start();
            _listenerTask = Task.Run(() => AcceptConnections(_cancellationSource.Token));
            _log.LogDebug("Started");
        }

        private async Task AcceptConnections(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_tcpListener.Pending())
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    //todo: we need to keep more info per connection than just the Task
                    _connections.Add(Task.Run(() => HandleConnection(tcpClient)));
                    _log.LogInformation("Connection from {0}", tcpClient.Client.RemoteEndPoint);
                }
                else await DelayEx(TimeSpan.FromMilliseconds(40), cancellationToken);
            }
        }

        private static async Task DelayEx(TimeSpan timeSpan, CancellationToken cancellationToken, bool throwOnCancel = false)
        {
            try
            {
                await Task.Delay(timeSpan, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                if (throwOnCancel) throw;
            }
        }

        private async Task HandleConnection(TcpClient tcpClient)
        {

            var serializer = _config.GetSerializer();
            var stream = tcpClient.GetStream();

            var outgoingMessages = new BlockingCollection<NetworkMessage>();
            var writerTask = Task.Run(() => SendMessages(outgoingMessages, stream));
            var serverProtocol = new ServerProtocol<T>(_engine);
            serverProtocol.OnMessage += outgoingMessages.Add;

            while (!_cancellationSource.Token.IsCancellationRequested)
            {
                var message = await ReadNetworkMessage(stream, serializer);
                serverProtocol.Handle(message);
            }
            //Assuming all methods are sync, then the following is not necessary
            //serverProtocol.Dispose();
            outgoingMessages.CompleteAdding();
            await writerTask;
        }

        private async Task SendMessages(BlockingCollection<NetworkMessage> messages, Stream stream)
        {
            var serializer = _config.GetSerializer();
            var cancellationToken = _cancellationSource.Token;
            var messageId = 0;
            while (messages.IsCompleted)
            {
                var message = messages.Take(cancellationToken);
                var bytes = serializer.Serialize(message);
                var packet = new Packet
                {
                    Payload = bytes,
                    MessageId = ++messageId,
                    Size = bytes.Length + 10
                };
                await packet.WriteTo(stream);
                await stream.FlushAsync();
            }
        }
        
        private async Task<NetworkMessage> ReadNetworkMessage(Stream stream, ISerializer serializer)
        {
            MemoryStream buffer = new MemoryStream();
            while (true)
            {
                var packet = await Packet.ReadAsync(stream, _cancellationSource.Token);
                buffer.Write(packet.Payload,0, packet.Payload.Length);
                if (packet.IsTerminal) break;
            }
            buffer.Position = 0;
            return (NetworkMessage) serializer.ReadObject(buffer);
        }

        public void Stop()
        {
            _log.LogInformation("Closing");
            _cancellationSource.Cancel();
            _listenerTask.Wait();
            _tcpListener.Stop(); //todo: should we stop before Cancel() and Wait() ?
            _log.LogDebug("Closed");
        }
    }

    public interface IHandle<in T>
    {
        void Handle(T message);
    }

    [Flags]
    internal enum PacketInfo : Int16
    {
        IsTerminal = 1,

    }

    internal class CommandRequest : NetworkMessage
    {
        public Command Command { get; set; }
    }

    internal class QueryRequest : NetworkMessage
    {
        public Query Query { get; set; }
    }
    
    internal abstract class NetworkMessage
    {
    }

    internal class Ping: NetworkMessage
    {
        public Guid Id { get; }

        public Ping(Guid id)
        {
            Id = id;
        }
    }

    internal class Pong : NetworkMessage
    {
        public Guid Id { get; }

        public Pong(Guid id)
        {
            Id = id;
        }
    }
}
