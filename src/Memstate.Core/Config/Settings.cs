using System;
using System.IO;
using System.Reflection;

namespace Memstate
{
    public abstract class Settings
    {
        private const string MsConfigSettingsProviderType = "Memstate.MsConfig.MsConfigSettingsProvider, Memstate.MsConfig";

        public abstract string Key { get; }

        public static T Get<T>() where T : Settings, new()
        {
            Initialize();
            return Provider.Get<T>();
        }

        public static SettingsProvider Provider { get; set; }


        public static void Initialize()
        {
            if (Provider != null) return;
            var providerType = Type.GetType(MsConfigSettingsProviderType, throwOnError: false);
            providerType = providerType ?? typeof(NullSettingsProvider);
            Provider = (SettingsProvider)Activator.CreateInstance(providerType);
            MemstateSettings.Current = Get<MemstateSettings>();
        }

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