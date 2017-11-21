namespace Memstate.Models.Redis
{
    public class Range
    {
        public readonly int FirstIdx;
        public readonly int LastIdx;
        public readonly int Length;

        public Range(int first, int last, int relativeTo = 0)
        {
            FirstIdx = first < 0 ? first + relativeTo : first;
            LastIdx = last < 0 ? last + relativeTo : last;
            Length = LastIdx - FirstIdx + 1;
        }

        public Range Flip(int relativeTo)
        {
            return new Range(relativeTo - LastIdx - 1, relativeTo - FirstIdx - 1, relativeTo);
        }
    }
}