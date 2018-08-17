using System;
using Memstate.Configuration;

namespace Memstate
{
    public class MemstateSettings : Settings
    {
        public override string Key { get; } = "Memstate";

        public MemstateSettings()
            : this(Config.Current)
        {
        }

        public MemstateSettings(Config config)
        {
            config.Bind(this, Key);
        }

        /// <summary>
        /// Maximum number of commands per batch sent to journal writer
        /// </summary>
        public int MaxBatchSize { get; set; } = 1024;

        public string StreamName { get; set; } = "memstate";
        
        public int MaxBatchQueueLength { get; set; } = int.MaxValue;

        public bool AllowBrokenSequence { get; set; } = false;

        public string Model { get; set; } = typeof(Models.KeyValueStore<int>).AssemblyQualifiedName;

        public string ModelCreator { get; set; } = typeof(DefaultModelCreator).AssemblyQualifiedName;

        public IModelCreator CreateModelCreator()
        {
            var type = Type.GetType(ModelCreator);
            var modelCreator = (IModelCreator) Activator.CreateInstance(type, Array.Empty<object>());
            return modelCreator;
        }
    }
}