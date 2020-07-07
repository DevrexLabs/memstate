namespace Memstate
{
    public class ClientSettings : Settings
    {
        public ClientSettings() : base("Memstate.Client") { }
        public ConnectionType Type { get; set; }

        public bool IsLocal => Type == ConnectionType.Local;

        public bool IsRemote => Type == ConnectionType.Remote;

        /// <summary>
        /// Ip or hostname of server to connect to
        /// </summary>
        /// <value>The host.</value>
        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 3001;
    }
}