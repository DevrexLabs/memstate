using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate.Tcp
{
    [Flags]
    internal enum PacketInfo : Int16
    {
        IsPartial = 1,
    }

    /// <summary>
    /// A packet is the atomic unit of data transferred server and client over tcp 
    /// </summary>
    internal class Packet
    {
        /// <summary>
        /// Total number of packet bytes
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// This is a bad name, fix it.
        /// </summary>
        public PacketInfo Info { get; set; }

        /// <summary>
        /// A large message can be split across multiple packets.
        /// MessageId identifies the message the packet belongs to.
        /// Packets are delivered in order and as a consecutive
        /// sequence (not interleaved with packets of a different message)
        /// </summary>
        public long MessageId { get; set; }
        
        /// <summary>
        /// The actual data, number of bytes is Size - HeaderSize
        /// </summary>
        public byte[] Payload;


        public bool IsPartial => Info.HasFlag(PacketInfo.IsPartial);

        /// <summary>
        /// Is this the last packet of the message?
        /// </summary>
        public bool IsTerminal => !IsPartial;

        /// <summary>
        /// Header size is fixed, we start by reading this number of bytes
        /// </summary>
        public const int HeaderSize = sizeof(PacketInfo) + sizeof(int) + sizeof(long);

        /// <summary>
        /// Read a single packet from a given stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static async Task<Packet> ReadAsync(Stream stream, CancellationToken cancellation)
        {
            var buf = new byte[HeaderSize];
            await FillBufferAsync(stream, buf, cancellation);

            var reader = new BinaryReader(new MemoryStream(buf, writable:false));

            var packet = new Packet()
            {
                Size = reader.ReadInt32(),
                Info = (PacketInfo) reader.ReadInt16(),
                MessageId = reader.ReadInt32()
            };

            var payloadSize = packet.Size - HeaderSize;
            packet.Payload = new byte[payloadSize];
            await FillBufferAsync(stream, packet.Payload, cancellation);
            return packet;
        }

        /// <summary>
        /// Read bytes from a stream into a buffer until the buffer is full
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        internal static async Task FillBufferAsync(Stream stream, byte[] buffer, CancellationToken cancellation)
        {
            int totalBytesRead = 0;
            while (totalBytesRead < buffer.Length)
            {
                var bytesRead = await stream.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead, cancellation);
                totalBytesRead += bytesRead;
                cancellation.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Write this packet to a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task WriteToAsync(Stream stream)
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

        public static Packet Create(byte[] payload, long messageId)
        {
            return new Packet
            {
                Payload = payload,
                Size = payload.Length + HeaderSize,
                MessageId = messageId
            };
        }
    }
}