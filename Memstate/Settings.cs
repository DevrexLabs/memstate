namespace Memstate
{
    using System;
    using Microsoft.Extensions.Configuration;

    public abstract class Settings
    {
        protected Settings(MemstateSettings parent, string configurationKey)
        {
            Ensure.NotNull(parent, nameof(parent));
            Ensure.NotNullOrEmpty(configurationKey, nameof(configurationKey));
            Memstate = parent;
            Configuration = parent.Configuration.GetSection(configurationKey);
            Configuration.Bind(this);
        }

        protected Settings(IConfiguration configuration)
        {
            Ensure.NotNull(configuration, nameof(configuration));
            Configuration = configuration;
            Configuration.Bind(this);
        }

        public IConfiguration Configuration { get; protected set; }

        public MemstateSettings Memstate { get; protected set; }

        public virtual void Validate()
        {
        }
    }
}