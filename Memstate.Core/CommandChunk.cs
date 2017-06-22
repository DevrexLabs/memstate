using System;

namespace Memstate.Core
{
    /// <summary>
    /// A chunk of commands to be persisted
    /// </summary>
    public class CommandChunk
    {
        /// <summary>
        /// Unique id of this block
        /// </summary>
        public Guid Id;

        /// <summary>
        /// Id of this block
        /// </summary>
        public ulong EngineSequenceNumber;

        /// <summary>
        /// Unique id of the engine which created this block
        /// </summary>
        public Guid Engine;

        /// <summary>
        /// Timestamp when the block was created
        /// </summary>
        public DateTimeOffset Created;

        /// <summary>
        /// Version of this block
        /// </summary>
        public int Version = 1;

        //The actual serialized data
        public Command[] Commands;
    }
}