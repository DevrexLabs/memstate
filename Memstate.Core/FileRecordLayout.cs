using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Memstate.Core
{
    /// <summary>
    /// Container for bytes to be written to the journal, takes options when
    /// <remarks>
    /// Header format:
    /// 1. Byte   : LayoutOptions
    /// 2. Int32  : Number of payload bytes
    /// 3. Byte[] : Payload bytes
    /// 4. Int16  : Number of Checksum bytes. Optional, only when using Checksums
    /// 5. Byte[] Checksum bytes. Optional, only when using Checksums
    /// </remarks>
    /// </summary>
    public class FileRecordLayout
    {
        private readonly LayoutOptions _options;
        private readonly ICompressor _compressor;
        private readonly HashAlgorithm _hasher;
        private readonly IEncryption _encryption;

        public FileRecordLayout(LayoutOptions options, ICompressor compressor = null, HashAlgorithm hasher = null, IEncryption encryption = null)
        {
            _options = options;
            _hasher = hasher;
            if (options.HasFlag(LayoutOptions.Checksum) && _hasher == null)
            {
                _hasher = MD5.Create();
            }
            _compressor = compressor;
            _encryption = encryption;
        }

        public byte[] Read(BinaryReader reader)
        {
            var initialPosition = reader.BaseStream.Position;

            LayoutOptions options = (LayoutOptions)reader.ReadByte();
            int length = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(length);

            if (options.HasFlag(LayoutOptions.Checksum))
            {
                var numChecksumBytes = reader.ReadInt16();
                var checksumBytes = reader.ReadBytes(numChecksumBytes);
                if (!checksumBytes.SequenceEqual(_hasher.ComputeHash(bytes)))
                {
                    throw new InvalidDataException("Bad checksum, record position: " + initialPosition);
                }
            }

            if (options.HasFlag(LayoutOptions.Encryted))
            {
                bytes = _encryption.Encrypt(bytes);
            }

            if (options.HasFlag(LayoutOptions.Compressed))
            {
                bytes = _compressor.Decompress(bytes);
            }

            return bytes;
        }

        public void Write(byte[] payload, BinaryWriter writer)
        {
            writer.Write((byte)_options);
            if (_options.HasFlag(LayoutOptions.Compressed))
            {
                payload = _compressor.Compress(payload);
            }
            if (_options.HasFlag(LayoutOptions.Encryted))
            {
                payload = _encryption.Encrypt(payload);
            }
            writer.Write(payload.Length);
            writer.Write(payload);

            if (_options.HasFlag(LayoutOptions.Checksum))
            {
                byte[] checksum = _hasher.ComputeHash(payload);
                writer.Write((Int16)checksum.Length);
                writer.Write(checksum);
            }
        }
    }
}