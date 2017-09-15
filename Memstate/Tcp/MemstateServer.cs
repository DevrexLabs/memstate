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
            var ip = IPAddress.Any;
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
                else await DelayOrCanceled(TimeSpan.FromMilliseconds(40), cancellationToken);
            }
        }

        //todo: to extension method
        private static async Task DelayOrCanceled(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(timeSpan, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                
            }
        }

        private async Task HandleConnection(TcpClient tcpClient)
        {
            var serializer = _config.GetSerializer();
            var stream = tcpClient.GetStream();

            var outgoingMessages = new BlockingCollection<NetworkMessage>();
            var writerTask = Task.Run(() => SendMessages(outgoingMessages, stream));
            var session = new Session<T>(_config,_engine);
            session.OnMessage += outgoingMessages.Add;

            while (!_cancellationSource.Token.IsCancellationRequested)
            {
                _log.LogDebug("Waiting for message");
                var message = await NetworkMessage.ReadAsync(stream, serializer, _cancellationSource.Token);
                _log.LogDebug("Received {0} from {1}", message, tcpClient.Client.RemoteEndPoint);
                session.Handle(message);
            }
            //Assuming all methods are sync, then the following is not necessary
            //serverProtocol.Dispose();
            outgoingMessages.CompleteAdding();
            await writerTask;
        }

        private async Task SendMessages(BlockingCollection<NetworkMessage> messages, Stream stream)
        {
            //todo: consider using MessageProcessor
            var serializer = _config.GetSerializer();
            var cancellationToken = _cancellationSource.Token;
            var messageId = 0;
            while (!messages.IsCompleted)
            {
                var message = messages.TakeOrDefault(cancellationToken);
                if (message == null) break;
                var bytes = serializer.Serialize(message);
                var packet = Packet.Create(bytes, ++messageId);
                await packet.WriteToAsync(stream);
                await stream.FlushAsync();
            }
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
        IsPartial = 1,
    }

    internal class CommandResponse : Response
    {
        public CommandResponse(object result, Guid responseTo)
            :base(responseTo)
        {
            Result = result;
        }

        public object Result { get; }
    }

    internal class CommandRequest : Request
    {
        public CommandRequest(Command command)
        {
            Command = command;
        }

        public Command Command { get; set; }
    }

    internal abstract class Request : NetworkMessage
    {
        public Guid Id { get; set; }

        protected Request()
        {
            Id = Guid.NewGuid();
        }
    }
    internal class QueryRequest : Request
    {
        public QueryRequest(Query query)
        {
            Query = query;
        }

        public Query Query { get;}
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
