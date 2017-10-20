using System;

namespace EventStore.Tests
{
    public class NullDisposable : IDisposable
    {
        private NullDisposable()
        {
            
        }
        public static NullDisposable Default = new NullDisposable();
        public void Dispose() { }
    }
}