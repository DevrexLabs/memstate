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
    }
}