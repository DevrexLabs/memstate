using System;
using System.Net;

namespace Memstate
{
    public class ServerSettings : Settings
    {
        public ServerSettings() 
            : base(bindingPath: "Memstate.Server")
        {
        }

        /// <summary>
        /// The native TCP port to listen on
        /// </summary>
        public int Port { get; set; } = 3001;

        /// <summary>
        /// IP or IPv6 address to bind to
        /// </summary>
        public string Ip { get; set; } = "0.0.0.0";

        /// <summary>
        /// Throw an exception if the settings are invalid
        /// </summary>
        public void EnsureValid()
        {
            if (Port < 1 || Port > 65535) throw new ArgumentOutOfRangeException(nameof(Port));
            if (!IPAddress.TryParse(Ip, out var dummy))
            {
                throw new ArgumentException("Invalid IP Address", nameof(Ip));
            }
        }


    }
}