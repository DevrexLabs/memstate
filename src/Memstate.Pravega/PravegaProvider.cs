using System;
using Grpc.Net.Client;
using Memstate.Configuration;

namespace Memstate.Pravega
{
    public class PravegaProvider : StorageProvider
    {
        private readonly PravegaGateway.PravegaGatewayClient _client;

        public override void Initialize()
        {
            var config = Config.Current;
            var settings = config.GetSettings<EngineSettings>();
            var parts = (settings.StreamName + "/mystream").Split("/");
            if (parts.Length != 2) throw new ArgumentException("Bad scope/stream: " + settings.StreamName);
            var (scope, stream) = (parts[0], parts[1]);

            var createScopeResponse = _client.CreateScope(new CreateScopeRequest
            {
                Scope = scope

            });
            if (createScopeResponse.Created)
            {
                Console.WriteLine("Created scope: " + scope);
            }
            var createStreamResponse = _client.CreateStream(new CreateStreamRequest
            {
                Scope = scope,
                Stream = stream,
                ScalingPolicy = new ScalingPolicy
                {
                    MinNumSegments = 1,
                    ScaleType = ScalingPolicy.Types.ScalingPolicyType.FixedNumSegments,
                }
            });
            if (createStreamResponse.Created)
            {
                Console.WriteLine("Create stream " + "myscope/mystream");
            }
        }
        public override IJournalReader CreateJournalReader()
        {
            var serializer = Config.Current.CreateSerializer();
            return new PravegaJournalReader(_client, serializer);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            var serializer = Config.Current.CreateSerializer();
            return new PravegaJournalWriter(_client, serializer);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new PravegaSubscriptionSource(_client);
        }

        public PravegaProvider()
        {
            // https://github.com/grpc/grpc-java/issues/6193#issuecomment-537745226
            // https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.0#call-insecure-grpc-services-with-net-core-client
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
//            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
            var channel = GrpcChannel.ForAddress("http://localhost:54672");
            _client = new PravegaGateway.PravegaGatewayClient(channel);
        }
    }
}