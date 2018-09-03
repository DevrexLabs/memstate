using System;

namespace Memstate
{
    public class EngineSettings : Settings
    {
        public override string Key { get; } = "Memstate:Engine";

        /// <summary>
        /// Maximum number of commands per batch sent to journal writer
        /// Adjust this value to control latency and throughput. 
        /// Lower values = lower latency / lower throughput
        /// </summary>
        public int MaxBatchSize { get; set; } = 1024;

        /// <summary>
        /// Used by storage providers to set the name of the storage entity.
        /// Will be used as filename, stream name, table name, etc
        /// </summary>
        public string StreamName { get; set; } = "memstate";

        /// <summary>
        /// Limit the number of commands in the queue waiting to be written to the journal.
        /// If commands are arriving faster than they can be written, they will be queued.
        /// By default, the there is no limit to the size of the queue.
        /// </summary>
        public int MaxBatchQueueLength { get; set; } = int.MaxValue;

        /// <summary>
        /// If set to true, Engine will halt if there is a gap in the stream of commands.
        /// </summary>
        public bool AllowBrokenSequence { get; set; } = false;

        /// <summary>
        /// Name of the model type
        /// </summary>
        //public string Model { get; set; } = typeof(Models.KeyValueStore<int>).AssemblyQualifiedName;

    }
}