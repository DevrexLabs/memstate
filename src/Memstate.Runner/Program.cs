using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Host;
using Memstate.Runner.Commands;

namespace Memstate.Runner
{

    /// <summary>
    /// This will evolve into some kind of CLI or interactive terminal
    /// </summary>
    class Program
    {
        internal static Config Config = Config.CreateDefault();
        
        private static readonly Dictionary<string, Type> Commands = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            {"redis", typeof(RedisServerCommand)},
            {"demo", typeof(DemoCommand)},
            {"benchmark", typeof(BenchmarkCommand)},
            {"read-benchmark", typeof(ReadBenchmarkCommand)},
            {"write-benchmark", typeof(WriteBenchmarkCommand)}
        };

        public static int Main(string[] arguments)
        {
            return MainAsync(arguments).Result;
        }

        private static async Task<int> MainAsync(string[] arguments)
        {
            var signal = new AutoResetEvent(false);

            var input = arguments.Length > 0 ? arguments[0] : "redis";

            arguments = arguments.Length > 0 ? arguments.Skip(1).ToArray() : arguments;

            if (!Commands.TryGetValue(input, out var type))
            {
                Console.WriteLine($"Invalid command: {input}");

                return -1;
            }

            var command = (ICommand)Activator.CreateInstance(type);

            command.Done += (sender, args) => signal.Set();
            OnExit.Register(() => signal.Set());

            try
            {
                await command.Start(arguments);

                signal.WaitOne();

                await command.Stop();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);

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
