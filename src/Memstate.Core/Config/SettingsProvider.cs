using System;

namespace Memstate
{
    public abstract class SettingsProvider
    {
        public abstract T Get<T>(string key = null) where T : Settings, new();
        public abstract void Bind(Settings settings, string key = null);
    }
}