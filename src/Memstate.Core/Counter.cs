using System.Threading;

namespace Memstate
{
    internal class Counter
    {
        private readonly long _initialValue;
        
        private long _value;

        public Counter(long initialValue = 0)
        {
            _initialValue = initialValue;
        }

        public long Value => Interlocked.Read(ref _value);

        public long Next()
        {
            return Interlocked.Increment(ref _value);
        }

        public long Reset()
        {
            return Interlocked.Exchange(ref _value, _initialValue);
        }
    }
}