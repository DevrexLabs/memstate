using System;
using System.IO;
using System.Threading.Tasks;
using Memstate.Models;
using Memstate.Tcp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Memstate.Host.Commands
{
    public class ServerCommand : ICommand
    {
        public event EventHandler Done = (sender, args) => { };
        
        private MemstateSettings _settings;

        private Engine<KeyValueStore<int>> _engine;

        private MemstateServer<KeyValueStore<int>> _server;

        private IWebHost _host;

        public async Task Start(string[] arguments)
        {
            InitializeSettings(arguments);

            StartServer();

            await StartWebInterface();
        }

        public async Task Stop()
        {
            await _host.StopAsync();

            _server.Stop();

            await _engine.DisposeAsync();
        }

        private void StartServer()
        {
            // TODO: The model should be decided by settings / reflection.
            _engine = new EngineBuilder(_settings).Build<KeyValueStore<int>>();

            _server = new MemstateServer<KeyValueStore<int>>(_settings, _engine);

            _server.Start();
        }

        private void InitializeSettings(string[] arguments)
        {
            _settings = new MemstateSettings(arguments);

            // TODO: This should be decided by the appsettings.json or the arguments.
            _settings.WithInmemoryStorage();

            _settings.LoggerFactory.AddConsole((category, level) => level > LogLevel.Debug);
        }

        private async Task StartWebInterface()
        {
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Web.Startup>()
                .ConfigureServices(services => services.AddSingleton(_settings))
                .Build();

            await _host.StartAsync();
        }
    }
}