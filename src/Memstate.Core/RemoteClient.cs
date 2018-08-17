using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Tcp;
using Memstate.Logging;
using Memstate.Configuration;

namespace Memstate
{
    public class RemoteClient<TModel> : Client<TModel> where TModel : class
    {
        private readonly EngineSettings _settings;

        private readonly ILog _logger;

        /// <summary>
        /// The tcp connection to the server
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// SerializerName used to serialize outgoing messages
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Serializer used to deserialize incoming messages
        /// </summary>
        private readonly ISerializer _deserializer;

        /// <summary>
        /// Stream that we read and write messages to/from
        /// </summary>
        private NetworkStream _stream;

        /// <summary>
        /// Requests that have been sent awaiting responses
        /// </summary>
        private readonly Dictionary<Guid, TaskCompletionSource<Message>> _pendingRequests;

        /// <summary>
        /// Queue of messages to be sent to the server
        /// </summary>
        private MessageProcessor<Message> _messageDispatcher;

        /// <summary>
        /// Task that processes incoming messages from the server
        /// </summary>
        private Task _messageHandler;

        private readonly Counter _counter = new Counter();

        private readonly CancellationTokenSource _cancellationSource;

        private readonly Dictionary<Type, Action<Event>> _eventHandlers;

        public RemoteClient()
        {
            var cfg = Config.Current;
            _settings = cfg.GetSettings<EngineSettings>();
            _serializer = cfg.CreateSerializer();
            _deserializer = cfg.CreateSerializer();
            _pendingRequests = new Dictionary<Guid, TaskCompletionSource<Message>>();
            _logger = LogProvider.GetCurrentClassLogger();
            _cancellationSource = new CancellationTokenSource();
            _eventHandlers = new Dictionary<Type, Action<Event>>();
        }


        public async Task Connect(string host = "localhost", int port = 3001)
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port);
            _stream = _tcpClient.GetStream();
            _logger.Info($"Connected to {host}:{port}");
            _messageDispatcher = new MessageProcessor<Message>(WriteMessage);
            _messageHandler = Task.Run(ReceiveMessages);
        }

        private void Handle(Message message)
        {
            switch (message)
            {
                case CommandResponse respose:
                    Handle(respose);
                    break;

                case QueryResponse response:
                    Handle(response);
                    break;

                case EventsRaised response:
                    Handle(response);
                    break;

                case SubscribeResponse response:
                    Handle(response);
                    break;

                case UnsubscribeResponse response:
                    Handle(response);
                    break;

                default:
                    _logger.Error("No handler for message " + message);
                    break;
            }
        }

        private void Handle(Response response)
        {
            var requestId = response.ResponseTo;

            CompleteRequest(requestId, response);
        }

        private void Handle(EventsRaised eventsRaised)
        {
            foreach(var @event in eventsRaised.Events)
            {
                if (_eventHandlers.TryGetValue(@event.GetType(), out var handler))
                {
                    handler.Invoke(@event);
                }
                else
                {
                    _logger.Error("No handler for event type {0}", @event);
                }
            }
        }

        private void CompleteRequest(Guid requestId, Response response)
        {
            if (_pendingRequests.TryGetValue(requestId, out var completionSource))
            {
                completionSource.SetResult(response);
                _pendingRequests.Remove(requestId);
            }
            else
            {
                _logger.Error($"No completion source for {response}, id {response.ResponseTo}");
            }
        }

        private async Task ReceiveMessages()
        {
            _logger.Trace("Starting ReceiveMessages task");
            var cancellationToken = _cancellationSource.Token;

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.Trace("awaiting NetworkMessage");
                var message = await Message.Read(_stream, _deserializer, cancellationToken);
                _logger.Debug("message received " + message);
                if (message == null) break;
                Handle(message);
            }
        }

        /// <summary>
        /// This method is called by the message processor never call it directly!
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task WriteMessage(Message message)
        {
            _logger.Debug("WriteMessage: invoked with " + message);
            var bytes = _serializer.Serialize(message);
            _logger.Debug("WriteMessage: serialized message size: " + bytes.Length);

            var messageId = _counter.Next();
            var packet = Packet.Create(bytes, messageId);

            await packet.WriteTo(_stream);
            _logger.Trace("Packet written");
            await _stream.FlushAsync();
        }

        private Task<Message> SendAndReceive(Request request)
        {
            var completionSource = new TaskCompletionSource<Message>();
            _pendingRequests[request.Id] = completionSource;
            _logger.Trace("SendAndReceive: queueing request id {0}, type {1}", request.Id, request.GetType());
            _messageDispatcher.Enqueue(request);
            return completionSource.Task;
        }

        internal async override Task<object> ExecuteUntyped(Query query)
        {
            var request = new QueryRequest(query);
            var response = (QueryResponse)await SendAndReceive(request);
            return response.Result;
        }


        public override Task Execute(Command<TModel> command)
        {
            var request = new CommandRequest(command);
            return SendAndReceive(request);
        }

        public override async Task<TResult> Execute<TResult>(Command<TModel, TResult> command)
        {
            var request = new CommandRequest(command);
            var response = (CommandResponse) await SendAndReceive(request);
            return (TResult) response.Result;
        }

        public override async Task<TResult> Execute<TResult>(Query<TModel, TResult> query)
        {
            var request = new QueryRequest(query);
            var response = (QueryResponse) await SendAndReceive(request);
            return (TResult) response.Result;
        }

        public override Task Unsubscribe<T>()
        {
            return SendAndReceive(new UnsubscribeRequest(typeof(T)));
        }

        public override Task Subscribe<T>(Action<T> handler, IEventFilter filter = null)
        {
            return SendAndReceive(new SubscribeRequest(typeof(T), filter));
        }

        public void Dispose()
        {
            _messageDispatcher.Dispose();
            _tcpClient.Dispose();
        }
    }
}