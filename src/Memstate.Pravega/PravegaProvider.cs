using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Memstate.Configuration;

namespace Memstate.Pravega
{
    public class PravegaProvider : IStorageProvider
    {
        private readonly PravegaGateway.PravegaGatewayClient _client;

        private string _scope;
        private string _stream;

        public async Task Provision()
        {
            var config = Config.Current;
            var settings = config.GetSettings<EngineSettings>();
            var streamName = settings.StreamName;

            if (!streamName.Contains("/")) streamName += "/stream";
 
            var parts = streamName.Split("/");
            if (parts.Length != 2) throw new ArgumentException("Bad scope/stream: " + settings.StreamName);
            (_scope, _stream) = (parts[0], parts[1]);

            var request = new CreateScopeRequest { Scope = _scope };
            await _client.CreateScopeAsync(request);

            await _client.CreateStreamAsync(new CreateStreamRequest
            {
                Scope = _scope,
                Stream = _stream,
                ScalingPolicy = new ScalingPolicy
                {
                    MinNumSegments = 1,
                    ScaleType = ScalingPolicy.Types.ScalingPolicyType.FixedNumSegments,
                }
            });
        }

        public IJournalReader CreateJournalReader()
        {
            var serializer = Config.Current.CreateSerializer();
            return new PravegaJournalReader(_client, serializer, _scope, _stream, null);
        }

        public IJournalWriter CreateJournalWriter()
        {
            var serializer = Config.Current.CreateSerializer();
            return new PravegaJournalWriter(_client, serializer, _scope, _stream);
        }
        
        public PravegaProvider()
        {
            // https://github.com/grpc/grpc-java/issues/6193#issuecomment-537745226
            // https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.0#call-insecure-grpc-services-with-net-core-client
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var channel = GrpcChannel.ForAddress("http://127.0.0.1:54672");
            _client = new PravegaGateway.PravegaGatewayClient(channel);
        }
    }
}