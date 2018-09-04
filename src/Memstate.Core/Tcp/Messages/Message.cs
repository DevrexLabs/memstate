using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate.Tcp
{
    /// <summary>
    /// Base class for messages passed between client and server
    /// </summary>
    internal abstract class Message
    {
        protected Message()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public static async Task<Message> Read(
            Stream stream,
            ISerializer serializer,
            CancellationToken cancellationToken)
        {
            var buffer = new MemoryStream();

            while (true)
            {
                var packet = await Packet.Read(stream, cancellationToken);

                buffer.Write(packet.Payload, 0, packet.Payload.Length);

                if (packet.IsTerminal)
                {
                    break;
                }
            }

            buffer.Position = 0;

            return (Message) serializer.ReadObject(buffer);
        }
    }
}