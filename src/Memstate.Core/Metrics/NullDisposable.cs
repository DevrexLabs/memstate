using System;

namespace Memstate
{
    public sealed class NullDisposable : IDisposable
    {
        /// <summary>
        /// The singleton instance of this class
        /// </summary>
        public static readonly NullDisposable Instance = new NullDisposable();

        /// <summary>
        /// Private constructor enforces singleton pattern
        /// </summary>
        private NullDisposable() { }

        /// <summary>
        /// Do nothing
        /// </summary>
        public void Dispose() { }
    }
}