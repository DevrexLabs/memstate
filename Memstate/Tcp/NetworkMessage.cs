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
                var packet = await Packet.ReadAsync(stream, cancellationToken);
                buffer.Write(packet.Payload, 0, packet.Payload.Length);
                if (packet.IsTerminal) break;
            }
            buffer.Position = 0;
            return (NetworkMessage)serializer.ReadObject(buffer);
        }
    }
}