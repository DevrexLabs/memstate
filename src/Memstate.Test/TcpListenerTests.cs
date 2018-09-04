using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Memstate.Test
{
    public class TcpListenerTests
    {
        /// <summary>
        /// Verifies the assumption that the blocking TcpListener
        /// Accept*Async calls will return when the listener is stopped.
        /// </summary>
        [Test]
        public void Accept_throws_when_Socket_is_closed()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 43675);
            listener.Start();
            Task<TcpClient> task = listener.AcceptTcpClientAsync();
            Task.Delay(1000);
            listener.Stop();
            Assert.Throws<AggregateException>(() =>
            {
                TcpClient unused = task.Result;
            });
        }
    }
}