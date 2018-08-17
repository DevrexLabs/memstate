using System;
using System.IO;
using System.Reflection;

namespace Memstate
{
    public abstract class Settings
    {
        private const string MsConfigSettingsProviderType = "Memstate.MsConfig.MsConfigSettingsProvider, Memstate.MsConfig";

        public abstract string Key { get; }

        public virtual void Validate()
        {
        }

        public string GetEmbeddedResource(string resourceName)
        {
            var assembly = GetAssemblyOfExecutingType();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public string[] GetEmbeddedResourceNames()
        {
            var assembly = GetAssemblyOfExecutingType();
            return assembly.GetManifestResourceNames();
        }

        /// <summary>
        /// Get the assembly of the concrete subclass
        /// </summary>
        /// <returns>The assembly of executing type.</returns>
        private Assembly GetAssemblyOfExecutingType()
        {
            return GetType().GetTypeInfo().Assembly;
        }
    }
}