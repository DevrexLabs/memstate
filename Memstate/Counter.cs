using System.Threading;

namespace Memstate
{
    public class Counter
    {
        private long _value;

        public long Next()
        {
            return Interlocked.Increment(ref _value);
        }

        public long Value => _value;
    }
}