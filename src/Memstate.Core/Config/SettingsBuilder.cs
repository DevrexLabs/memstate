namespace Memstate
{
    public abstract class SettingsBuilder
    {
        public abstract T Build<T>(string key = null) where T : Settings, new();
        public abstract void Bind(Settings settings, string key = null);
        public static SettingsBuilder Current { get; set; } 
            = new NullSettingsBuilder();
    }
}