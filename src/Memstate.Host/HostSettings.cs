namespace Memstate.Host
{
    public class HostSettings : Settings
    {
        public HostSettings() : base("Memstate.Host") { }

        public bool WebConsoleEnabled
        {
            get;
            set;
        }
    }
}