namespace Memstate
{
    public class NullSettingsProvider : SettingsProvider
    {
        public override void Bind(Settings settings, string key = null)
        {
        }

        public override T Get<T>(string key = null) => new T();
    }
}