using Memstate.Configuration;

namespace Memstate.Host
{
    public class HostBuilder<TModel> where TModel : class
    {
        readonly HostSettings _settings;

        public HostBuilder()
        {
            _settings = Config.Current.GetSettings<HostSettings>();
        }

        public Host<TModel> Build()
        {
            return new Host<TModel>(_settings);
        }

        public HostBuilder<TModel> UseWebConsole()
        {
            _settings.WebConsoleEnabled = true;
            return this;
        }
    }
}