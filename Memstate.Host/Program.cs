using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using App.Metrics;
using App.Metrics.Filtering;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Json.Extensions;
using Memstate.Models;
using Memstate.Models.KeyValue;
using Memstate.Postgresql;
using Memstate.Tcp;
using Microsoft.Extensions.Logging;

namespace Memstate.Host
{
    public static class Program
    {
        private static readonly Dictionary<string, Action> Commands = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
        {
            {"client", RunClient},
            {"server", RunServer},
            {"metrics", Metrics},
            {"help", Help},
            {"quit", () => _running = false},
            {"exit", () => _running = false}
        };

        private static bool _running = true;

        public static void Main(string[] args)
        {
            Console.WriteLine("Memstate Console");

            while (_running)
            {
                Console.Write("> ");

                var input = Console.ReadLine();

                if (Commands.TryGetValue(input, out var command))
                {
                    command();
                }
                else
                {
                    Console.WriteLine($"Invalid command '{input}, please try again.");
                }
            }
        }

        private static void Help()
        {
            Console.WriteLine("Available commands:");

            foreach (var command in Commands)
            {
                Console.WriteLine($"\t{command.Key}");
            }

            Console.WriteLine();
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
            MemstateSettings settings = new MemstateSettings();
            settings.FileSystem = new InMemoryFileSystem();
            settings.LoggerFactory.AddConsole((category, level) => true);
            var engine = new EngineBuilder(settings).Build<KeyValueStore<int>>();
            var server = new MemstateServer<KeyValueStore<int>>(settings, engine);
            server.Start();
            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine("Type exit to quit");
            }
            server.Stop();
            Console.WriteLine("Server stopped, hit enter to terminate");
            Console.ReadLine();
        }

        private static void Metrics()
        {
            var random = new Random();

            Console.WriteLine("Starting an engine to generate traffic");

            var settings = new MemstateSettings();

            settings.WithRandomSuffixAppendedToStreamName();

            settings.UsePostgresqlProvider();
            settings.LoggerFactory.AddConsole((category, level) => true);

            settings.Configuration["StorageProviders:Postgresql:ConnectionString"] = "Host=localhost; User ID=hagbard; Database=postgres;";

            var engine = new EngineBuilder(settings).Build<KeyValueStore<int>>();

            var low = 0;
            var high = random.Next(100, 200);

            for (var i = low; i < high; i++)
            {
                var command = new Set<int>($"key-{i}", i);

                engine.Execute(command);
            }

            for (var i = low; i < random.Next(low, high); i++)
            {
                var query = new Get<int>($"key-{random.Next(low, high)}");

                engine.Execute(query);
            }

            PrintMetrics(settings.Metrics.Snapshot.Get(), settings.Metrics.DefaultOutputMetricsFormatter);

            engine.DisposeAsync().Wait();
        }

        private static void PrintMetrics(MetricsDataValueSource snapshot, IMetricsOutputFormatter formatter)
        {
            using (var stream = new MemoryStream())
            {
                formatter.WriteAsync(stream, snapshot).Wait();

                var result = Encoding.UTF8.GetString(stream.ToArray());

                Console.WriteLine(result);
            }
        }
    }
}