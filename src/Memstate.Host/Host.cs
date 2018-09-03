using System.IO;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Tcp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Memstate.Host
{

    /// <summary>
    /// Service host for a MemstateServer and an optional web console
    /// </summary>
    public class Host<TModel> where TModel: class
    {
        MemstateServer<TModel> _server;
        IWebHost _webConsole;
        HostSettings _settings;

        public Engine<TModel> TheEngine { get; private set; }

        public Host(HostSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Start the Host in the background,
        /// the Task completes when the host is ready to accept connections
        /// </summary>
        public async Task Start()
        {
            TheEngine = await Engine.Start<TModel>();
            _server = new MemstateServer<TModel>(TheEngine);
            _server.Start();
            await StartWebConsole();
        }

        public Task Stop()
        {
            return Task.WhenAll(
                _server.Stop(),
                StopWebConsole());
        }

        private Task StopWebConsole()
        {
            if (!_settings.WebConsoleEnabled) return Task.CompletedTask;
            return _webConsole.StopAsync();
        }

        private Task StartWebConsole()
        {
            if (!_settings.WebConsoleEnabled) return Task.CompletedTask;

            _webConsole = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Web.Startup>()
                .ConfigureServices(ConfigureServices)
                .Build();

            return _webConsole.StartAsync();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var settings = Config.Current.GetSettings<EngineSettings>();
            services.AddSingleton(settings);
            services.AddSingleton(TheEngine);
        }

    }
}