using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate.Tcp
{
    internal class Packet
    {
        /// <summary>
        /// Total number of packet bytes
        /// </summary>
        public int Size { get; set; }


        public PacketInfo Info { get; set; }

        /// <summary>
        /// A large message can be split across multiple packets
        /// </summary>
        public long MessageId { get; set; }
        
        /// <summary>
        /// A chunk of the serialized message
        /// </summary>
        public byte[] Payload;


        public bool IsPartial => Info.HasFlag(PacketInfo.IsPartial);
        public bool IsTerminal => !IsPartial;

        public static async Task<Packet> ReadAsync(Stream stream, CancellationToken cancellation)
        {
            const int headerSize = sizeof(PacketInfo) + sizeof(int) + sizeof(long);

            Console.WriteLine("packet.readasync, before await FillBuffer header");
            var buf = new byte[headerSize];
            await FillBuffer(stream, buf, cancellation);
            Console.WriteLine("packet.readasync, after await FillBuffer header");

            var reader = new BinaryReader(new MemoryStream(buf));

            var packet = new Packet()
            {
                Size = reader.ReadInt32(),
                Info = (PacketInfo) reader.ReadInt16(),
                MessageId = reader.ReadInt32()
            };

            Console.WriteLine("packet.readasync, before await FillBuffer payload");
            var payloadSize = packet.Size - headerSize;
            packet.Payload = new byte[payloadSize];
            await FillBuffer(stream, packet.Payload, cancellation);
            Console.WriteLine("packet.readasync, after await FillBuffer payload");
            return packet;
        }

        internal static async Task FillBuffer(Stream stream, byte[] buffer, CancellationToken cancellation)
        {
            int bytesRead = 0;
            while (bytesRead < buffer.Length)
            {
                bytesRead += await stream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead, cancellation);
            }
        }

        public async Task WriteTo(Stream stream)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            writer.Write(Size);
            writer.Write((short)Info);
            writer.Write(MessageId);
            writer.Write(Payload);
            writer.Flush();
            ms.Position = 0;
            await ms.CopyToAsync(stream);
        }

        /// <summary>
        /// Use static Create to guarantee integrity
        /// </summary>
        private Packet(){}

        public static Packet Create(byte[] payload, long messageId)
        {
            return new Packet
            {
                Payload = payload,
                Size = payload.Length + 10,
                MessageId = messageId
            };
        }
    }
}