using System.IO;
using System.Reflection;

namespace Memstate
{
    public abstract class Settings
    {
        public abstract string Key { get; }

        public static T Get<T>() where T : Settings, new()
        {
            var builder = SettingsBuilder.Current;
            Ensure.NotNull(builder, "SettingsBuilder.Current");
            return builder.Build<T>();
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