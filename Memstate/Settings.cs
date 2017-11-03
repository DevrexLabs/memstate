namespace Memstate
{
    using System;
    using Microsoft.Extensions.Configuration;

    public abstract class Settings
    {
        protected Settings(Settings parent, string configurationKey)
        {
            Ensure.NotNull(parent, nameof(parent));
            Ensure.NotNullOrEmpty(configurationKey, nameof(configurationKey));

            Configuration = parent.Configuration.GetSection(configurationKey);
            Configuration.Bind(this);
        }

        protected Settings(IConfiguration configuration)
        {
            Ensure.NotNull(configuration, nameof(configuration));
            Configuration = configuration;
            Configuration.Bind(this);
        }

        protected Settings(string configurationKey, params string[] commandLineArguments)
        {
            Ensure.NotNullOrEmpty(configurationKey, nameof(configurationKey));

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(commandLineArguments ?? Array.Empty<string>())
                .Build()
                .GetSection(configurationKey);
            Configuration.Bind(this);
        }

        // https://msdn.microsoft.com/en-us/magazine/mt632279.aspx
        public IConfiguration Configuration { get; protected set; }

        public virtual void Validate()
        {
        }
    }
}