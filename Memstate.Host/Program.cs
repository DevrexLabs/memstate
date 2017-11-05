using System;
using Memstate.Models;
using Memstate.Models.KeyValue;
using Memstate.Tcp;
using Microsoft.Extensions.Logging;

namespace Memstate.Host
{
    using System.Threading.Tasks;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Memstate Console");
            var command = string.Empty;
            while (true)
            {
                Console.Write("'server' or 'client' : ");
                command = Console.ReadLine();
                if (command == "server" || command == "client") break;
                Console.WriteLine("bad command, try again");
            }
            if (command == "server") RunServer();
            else RunClient();
        }

        private static void RunClient()
        {
            MemstateSettings config = new MemstateSettings();
            config.LoggerFactory.AddConsole((category, level) => true);

            Console.Write("Connecting to localhost:3001 ... ");
            var client = new MemstateClient<KeyValueStore<int>>(config);
            client.ConnectAsync().Wait();
            Console.WriteLine("connected!");

            Console.WriteLine("Set 'KEY' = 42");
            var cmd = new Set<int>("KEY", 42);
            var version = client.ExecuteAsync(cmd).Result;
            Console.WriteLine("Version returned: " + version);

            Console.WriteLine("Get 'KEY'");
            var query = new Get<int>("KEY");
            var node = client.ExecuteAsync(query).Result;
            Console.WriteLine($"Value: {node.Value}, version: {node.Version}");

            Console.WriteLine("Hit enter to exit");
            Console.ReadLine();

        }

        private static void RunServer()
        {
            Console.WriteLine("Starting server on port 3001, type exit to quit");
            MemstateSettings config = new MemstateSettings();
            config.StorageProvider = typeof(InMemoryStorageProvider).FullName;
            config.LoggerFactory.AddConsole((category,level) => true);
            var engine = new EngineBuilder(config).Build<KeyValueStore<int>>();
            var server = new MemstateServer<KeyValueStore<int>>(config, engine);
            server.Start();
            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine("Type exit to quit");
            }
            server.Stop();
            Console.WriteLine("Server stopped, hit enter to terminate");
            Console.ReadLine();
        }
    }
}
