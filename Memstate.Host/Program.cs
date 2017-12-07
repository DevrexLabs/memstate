using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Formatters;
using Memstate.Host.Commands;
using Memstate.Models;
using Memstate.Models.KeyValue;
using Memstate.Tcp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Memstate.Host
{
    public static class Program
    {
        private static readonly Dictionary<string, Type> Commands = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            {"demo", typeof(DemoCommand)},
            {"server", typeof(ServerCommand)},
            {"benchmark", typeof(BenchmarkCommand)},
            {"read-benchmark", typeof(ReadBenchmarkCommand)},
            {"write-benchmark", typeof(WriteBenchmarkCommand)}
        };

        public static int Main(string[] arguments)
        {
            return MainAsync(arguments).GetAwaiter().GetResult();
        }

        private static async Task<int> MainAsync(string[] arguments)
        {
            var signal = new AutoResetEvent(false);

            var input = arguments.Length > 0 ? arguments[0] : "server";

            arguments = arguments.Length > 0 ? arguments.Skip(1).ToArray() : arguments;

            if (!Commands.TryGetValue(input, out var type))
            {
                Console.WriteLine($"Invalid command: {input}");

                return -1;
            }
            
            var command = (ICommand) Activator.CreateInstance(type);

            command.Done += (sender, args) => signal.Set();
            Console.CancelKeyPress += (sender, args) => signal.Set();

            try
            {
                await command.Start(arguments);

                signal.WaitOne();

                await command.Stop();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return -1;
            }

            return 0;
        }

        /*private static Task RunClient(string[] arguments)
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

            return Task.CompletedTask;
        }*/
    }
}