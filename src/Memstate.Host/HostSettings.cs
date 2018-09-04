namespace Memstate.Host
{
    public class HostSettings : Settings
    {
        public override string Key => "Memstate:Host";

        public bool WebConsoleEnabled
        {
            get;
            set;
        }
    }
}