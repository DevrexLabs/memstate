namespace Memstate
{
    public class NullSettingsBuilder : SettingsBuilder
    {
        public override void Bind(Settings settings, string key = null)
        {
        }

        public override T Build<T>(string key = null) => new T();
    }
}