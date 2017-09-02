using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Memstate.Models;
using Memstate.Tcp;

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
            Console.WriteLine("Connecting to localhost:3001");
        }
        static void RunServer()
        {
            Console.WriteLine("Starting server on port 3001");
            Config config = new Config();
            var engine = new InMemoryEngineBuilder(config).Build<KeyValueStore<int>>();
            var server = new MemstateServer<KeyValueStore<int>>(config, engine);
            server.Start();
            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine("Type exit to quit");
            }
            server.Stop();

        }
    }
}
