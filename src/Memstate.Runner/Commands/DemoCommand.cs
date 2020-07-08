using System;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Host;
using Memstate.Models;
using Memstate.Models.KeyValue;

namespace Memstate.Runner.Commands
{

    /// <summary>
    /// Start a host and run queries and commands in separate tasks.
    /// Have a look at the web console, there should be some metrics.
    /// </summary>
    public class DemoCommand : ICommand
    {
        /// <summary>
        /// DemoCommand does not terminate so this event never fires.
        /// </summary>
        public event EventHandler Done = (sender, args) => { };
        
        private CancellationTokenSource _cancellationTokenSource;
        
        private Engine<KeyValueStore<int>> _engine;

        private Task _producer;
        private Task _consumer;

        public async Task Start(string[] arguments)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var cfg = Program.Config;
            cfg.FileSystem = new InMemoryFileSystem();

            var host = new HostBuilder<KeyValueStore<int>>(cfg)
                .UseWebConsole()
                .Build();

            await host.Start();
            _engine = host.TheEngine;

            _producer = Task.Run(Producer);
            _consumer = Task.Run(Consumer);
        }

        public async Task Stop()
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_producer, _consumer);
            await _engine.DisposeAsync();
        }

        private async Task Producer()
        {
            var random = new Random();

            await _engine.Execute(new Set<int>("key-0", 0));

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                for (var i = 0; i < random.Next(1, 100); i++)
                {
                    await _engine.Execute(new Set<int>($"key-{i}", i));
                }

                await Task.Delay(random.Next(100, 5000));
            }
        }

        private async Task Consumer()
        {
            var random = new Random();
            int? version = null;
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                for (var i = 0; i < random.Next(1, 100); i++)
                {
                    var node = await _engine.Execute(new Get<int>("key-0"));
                    if (version != node.Version)
                    {
                        version = node.Version;
                        var msg = $"key-0 changed. Value: {node.Value}, Version: {version}";
                        Console.WriteLine(msg);
                    }
                }

                await Task.Delay(random.Next(100, 3000));
            }
        }
    }
}