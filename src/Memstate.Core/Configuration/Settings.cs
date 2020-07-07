namespace Memstate
{
    /// <summary>
    /// Base class for settings
    /// </summary>
    public abstract class Settings
    {
        public string BindingPath { get; }
        protected Settings(string bindingPath) => BindingPath = bindingPath;
    }
}