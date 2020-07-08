using Memstate.Configuration;

namespace Memstate.Host
{
    public class HostBuilder<TModel> where TModel : class
    {
        readonly HostSettings _settings;
        private readonly Config _config;

        public HostBuilder(Config config = null)
        {
            _config = config ?? Config.CreateDefault();
            _settings = _config.GetSettings<HostSettings>();
        }

        public Host<TModel> Build()
        {
            return new Host<TModel>(_config, _settings);
        }

        public HostBuilder<TModel> UseWebConsole()
        {
            _settings.WebConsoleEnabled = true;
            return this;
        }
    }
}