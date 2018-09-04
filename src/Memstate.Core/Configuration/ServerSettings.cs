using System;
using System.Net;

namespace Memstate.Tcp
{
    public class ServerSettings : Settings
    {
        public override string Key => "Memstate:Server";

        /// <summary>
        /// The port to listen on
        /// </summary>
        public int Port { get; set; } = 3001;

        /// <summary>
        /// IP or IPv6 address to bind to
        /// </summary>
        public string Ip { get; set; } = "0.0.0.0";

        public override void Validate()
        {
            base.Validate();
            if (Port < 1 || Port > 65535) throw new ArgumentOutOfRangeException(nameof(Port));
            if (!IPAddress.TryParse(Ip, out var dummy))
            {
                throw new ArgumentException("Invalid IP Address", nameof(Ip));
            }
        }


    }
}