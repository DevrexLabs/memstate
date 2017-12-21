using System.Collections.Generic;
using System.Linq;

namespace Memstate
{
    public static class ArrayExtensions
    {
        public static IEnumerable<T[]> Split<T>(this T[] source, params int[] splitBy)
        {
            if (splitBy == null || splitBy.Length == 0)
            {
                splitBy = new[] {source.Length};
            }

            var taken = 0;

            var index = new Counter(0, splitBy.Length - 1);

            do
            {
                var chunk = source.Skip(taken).Take(splitBy[index.Next()]).ToArray();

                taken += chunk.Length;

                yield return chunk;
            }
            while (taken < source.Length);
        }

        private class Counter
        {
            private readonly int _maxSize;

            private int _value;

            public Counter(int initialValue = 0, int maxSize = int.MaxValue)
            {
                _value = initialValue;
                _maxSize = maxSize;
            }

            public int Next() => _value == _maxSize ? _maxSize : _value++;
        }
    }
}