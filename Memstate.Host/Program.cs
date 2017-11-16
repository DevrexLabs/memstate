using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Formatters;
using Memstate.Models;
using Memstate.Models.KeyValue;
using Memstate.Tcp;
using Microsoft.Extensions.Logging;

namespace Memstate.Host
{
    public static class Program
    {
        private static readonly Dictionary<string, Func<Task>> Commands = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
        {
            {"client", RunClient},
            {"server", RunServer},
            {"metrics", Metrics},
            {"help", Help},
            {"quit", Quit},
            {"exit", Quit},
            {"benchmark", Benchmark},
            {"read-benchmark", QueriesPerSecondBenchmark},
            {"write-benchmark", CommandsPerSecondBenchmark}
        };

        private static bool _running = true;

        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            Console.WriteLine("Memstate Console");

            while (_running)
            {
                Console.Write("> ");

                var input = Console.ReadLine() ?? string.Empty;

                if (Commands.TryGetValue(input, out var command))
                {
                    await command();
                }
                else
                {
                    Console.WriteLine($"Invalid command '{input}', please try again.");
                }
            }
        }

        private static Task Quit()
        {
            _running = false;

            return Task.CompletedTask;
        }

        private static Task Help()
        {
            Console.WriteLine("Available commands:");

            foreach (var command in Commands)
            {
                Console.WriteLine($"\t{command.Key}");
            }

            Console.WriteLine();

            return Task.CompletedTask;
        }

        private static Task RunClient()
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
        }

        private static Task RunServer()
        {
            Console.WriteLine("Starting server on port 3001, type exit to quit");
            var settings = new MemstateSettings
            {
                FileSystem = new InMemoryFileSystem()
            };
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

            return Task.CompletedTask;
        }

        private static async Task Metrics()
        {
            var random = new Random();

            Console.WriteLine("Starting an engine to generate traffic");

            var settings = new MemstateSettings();

            settings.WithRandomSuffixAppendedToStreamName();
            settings.LoggerFactory.AddConsole((category, level) => level != LogLevel.Debug);

            Console.WriteLine($"Settings: {settings}");

            var engine = new EngineBuilder(settings).Build<KeyValueStore<int>>();

            var low = 0;
            var high = random.Next(100, 200);

            for (var i = low; i < high; i++)
            {
                var command = new Set<int>($"key-{i}", i);

                await engine.ExecuteAsync(command);
            }

            for (var i = low; i < random.Next(low, high); i++)
            {
                var query = new Get<int>($"key-{random.Next(low, high)}");

                await engine.ExecuteAsync(query);
            }

            await PrintMetrics(settings.Metrics.Snapshot.Get(), settings.Metrics.DefaultOutputMetricsFormatter);

            await engine.DisposeAsync();
        }

        private static async Task PrintMetrics(MemstateSettings settings)
        {
            await PrintMetrics(settings.Metrics.Snapshot.Get(), settings.Metrics.DefaultOutputMetricsFormatter);
        }

        private static async Task PrintMetrics(MetricsDataValueSource snapshot, IMetricsOutputFormatter formatter)
        {
            using (var stream = new MemoryStream())
            {
                await formatter.WriteAsync(stream, snapshot);

                var result = Encoding.UTF8.GetString(stream.ToArray());

                Console.WriteLine(result);
            }
        }

        private static async Task Benchmark()
        {
            Console.WriteLine("Running commands per second benchmark:");
            await CommandsPerSecondBenchmark();
            Console.WriteLine();

            Console.WriteLine("Running queries per second benchmark:");
            await QueriesPerSecondBenchmark();
            Console.WriteLine();
        }

        private static async Task CommandsPerSecondBenchmark()
        {
            var settings = new MemstateSettings();

            settings.WithRandomSuffixAppendedToStreamName();
            settings.LoggerFactory.AddConsole(LogLevel.Information);

            Console.WriteLine($"Settings: {settings}");

            const int writes = 10000;
            
            var engine = new EngineBuilder(settings).Build<KeyValueStore<int>>();
            
            var totals = new TimeSpan[10];

            for (var run = 1; run <= 10; run++)
            {
                Console.WriteLine($"Run {run}");
                
                var stopwatch = Stopwatch.StartNew();

                var tasks = new Task[writes];

                for (var i = 0; i < writes; i++)
                {
                    tasks[i] = engine.ExecuteAsync(new Set<int>($"key-{i}", i));
                }

                Task.WaitAll(tasks);

                stopwatch.Stop();
                
                Console.WriteLine($"Executed {writes} commands in {stopwatch.Elapsed.TotalSeconds}s. {writes/ stopwatch.Elapsed.TotalSeconds:0.00} commands/second");

                totals[run - 1] = stopwatch.Elapsed;
            }
            
            await engine.DisposeAsync();

            var min = totals.Select(x => writes / x.TotalSeconds).Min();
            var max = totals.Select(x => writes / x.TotalSeconds).Max();
            var average = totals.Select(x => writes / x.TotalSeconds).Average();

            Console.WriteLine($"Min: {min:0.00} cps");
            Console.WriteLine($"Max: {max:0.00} cps");
            Console.WriteLine($"Average: {average:0.00} cps");

            await PrintMetrics(settings);
        }

        private static async Task QueriesPerSecondBenchmark()
        {
            var settings = new MemstateSettings();

            settings.WithRandomSuffixAppendedToStreamName();
            settings.LoggerFactory.AddConsole(LogLevel.Information);

            Console.WriteLine($"Settings: {settings}");

            const int reads = 1000000;

            var engine = new EngineBuilder(settings).Build<KeyValueStore<int>>();

            await engine.ExecuteAsync(new Set<int>("key-0", 0));

            var totals = new TimeSpan[10];

            for (var run = 1; run <= 10; run++)
            {
                Console.WriteLine($"Run {run}");

                var stopwatch = Stopwatch.StartNew();

                for (var i = 0; i < reads; i++)
                {
                    engine.Execute(new Get<int>("key-0"));
                }

                stopwatch.Stop();

                Console.WriteLine($"Read {reads} queries in {stopwatch.Elapsed.TotalSeconds}s. {reads / stopwatch.Elapsed.TotalSeconds:0.00} queries/second");

                totals[run - 1] = stopwatch.Elapsed;
            }

            await engine.DisposeAsync();

            var min = totals.Select(x => reads / x.TotalSeconds).Min();
            var max = totals.Select(x => reads / x.TotalSeconds).Max();
            var average = totals.Select(x => reads / x.TotalSeconds).Average();

            Console.WriteLine($"Min: {min:0.00} qps");
            Console.WriteLine($"Max: {max:0.00} qps");
            Console.WriteLine($"Average: {average:0.00} qps");

            await PrintMetrics(settings);
        }
    }
}