using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate.Tcp
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class NetworkMessage
    {
        public static async Task<NetworkMessage> ReadAsync(
            Stream stream, 
            ISerializer serializer, 
            CancellationToken cancellationToken)
        {
            MemoryStream buffer = new MemoryStream();
            while (true)
            {
                Console.WriteLine("networkmessage.readasync, before await packet.readasync");
                var packet = await Packet.ReadAsync(stream, cancellationToken);
                Console.WriteLine("networkmessage.readasync, after await packet.readasync");
                buffer.Write(packet.Payload, 0, packet.Payload.Length);
                Console.WriteLine("networkmessage.readasync, packet.IsTerminal: " + packet.IsTerminal);
                if (packet.IsTerminal) break;
            }
            Console.WriteLine("networkmessage.readasync, before serializer.ReadObject, buffer size: " + buffer.Length);
            buffer.Position = 0;
            return (NetworkMessage)serializer.ReadObject(buffer);
        }
    }
}