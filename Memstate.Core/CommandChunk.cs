using System;

namespace Memstate.Core
{
    /// <summary>
    /// A chunk of commands with some metadata
    /// </summary>
    public class CommandChunk
    {
        public CommandChunk(Command[] commands)
        {
            Commands = commands;
            Created = DateTimeOffset.Now;
        }

        /// <summary>
        /// Globally unique sequence number of this chunk
        /// 0 means not yet persisted
        /// </summary>
        public string GlobalSequenceNumber;


        public ulong LocalSequenceNumber;

        public string PartitionKey;

        /// <summary>
        /// Timestamp when the block was created
        /// </summary>
        public DateTimeOffset Created;

        /// <summary>
        /// Version of this block
        /// </summary>
        public int Version = 1;

        /// <summary>
        /// The actual commands
        /// </summary>
        public Command[] Commands;
    }
}