using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Memstate
{
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

        public string GetEmbeddedResource(string resourceName)
        {
            var assembly = GetType().GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public string[] GetEmbeddedResourceNames()
        {
            var assembly = GetType().GetTypeInfo().Assembly;

            return assembly.GetManifestResourceNames();
        }
    }
}