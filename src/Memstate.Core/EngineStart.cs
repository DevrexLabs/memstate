using System;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public static class Engine
    {

        /// <summary>
        /// Load an existing or create a new engine
        /// </summary>
        public static Task<Engine<T>> Start<T>() where T : class, new()
        {
            return new EngineBuilder().Build<T>();
        }

        /// <summary>
        /// Start the engine from an existing journal, 
        /// will throw if the journal doesn't exist
        /// </summary>
        public static Task<Engine<T>> Load<T>() where T: class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Start an engine with a new empty journal file, will
        /// throw if the journal already exists 
        /// </summary>
        /// <returns>The create.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static Task<Engine<T>> Create<T>() where T : class
        {
            throw new NotImplementedException();
        }
    }
}