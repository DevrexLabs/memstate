using Microsoft.Extensions.Configuration;

namespace Memstate
{
    public class MsConfigSettingsBuilder : SettingsBuilder
    {
        private readonly string[] _args;

        public IConfiguration Configuration { get; set; }

        public MsConfigSettingsBuilder(params string[] args)
        {
            _args = args;
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(_args)
                .Build();
        }

        public MsConfigSettingsBuilder(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public override T Build<T>(string key = null)
        {
            var settings = new T();
            Bind(settings, key);
            return settings;
        }

        public override void Bind(Settings settings, string key = null)
        {
            key = key ?? settings.Key;
            Configuration.Bind(key, settings);
        }
    }
}