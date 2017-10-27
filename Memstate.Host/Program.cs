using System;
using Memstate.Models;
using Memstate.Models.KeyValue;
using Memstate.Tcp;
using Microsoft.Extensions.Logging;

namespace Memstate.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Memstate Console");
            var command = "";
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

        static void RunClient()
        {
            Settings config = new Settings();
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

        static void RunServer()
        {
            Console.WriteLine("Starting server on port 3001, type exit to quit");
            Settings config = new Settings();
            config.LoggerFactory.AddConsole((category,level) => true);
            var engine = new InMemoryEngineBuilder(config).Build<KeyValueStore<int>>();
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
