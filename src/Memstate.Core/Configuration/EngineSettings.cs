using System;

namespace Memstate
{
    public class EngineSettings : Settings
    {
        public override string Key { get; } = "Memstate:Engine";

        /// <summary>
        /// Maximum number of commands per batch sent to journal writer
        /// </summary>
        public int MaxBatchSize { get; set; } = 1024;

        /// <summary>
        /// Used by storage providers to set the name of the storage entity.
        /// Will be used as filename, stream name, table name, etc
        /// </summary>
        public string StreamName { get; set; } = "memstate";
        
        public int MaxBatchQueueLength { get; set; } = int.MaxValue;

        /// <summary>
        /// If set to true, Engine will halt if there is a gap in the stream of commands.
        /// </summary>
        public bool AllowBrokenSequence { get; set; } = false;

        /// <summary>
        /// Name of the model type
        /// </summary>
        public string Model { get; set; } = typeof(Models.KeyValueStore<int>).AssemblyQualifiedName;

    }
}