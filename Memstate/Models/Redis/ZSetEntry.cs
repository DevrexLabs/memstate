using System;

namespace Memstate.Models.Redis
{
    public class ZSetEntry : IComparable<ZSetEntry>
    {
        public readonly string Member;
        public readonly double Score;

        public ZSetEntry(string member, double score)
        {
            Member = member;
            Score = score;
        }

        public int CompareTo(ZSetEntry other)
        {
            var result = Math.Sign(Score - other.Score);

            if (result == 0)
            {
                result = string.Compare(Member, other.Member, StringComparison.Ordinal);
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ZSetEntry;

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return Member == other.Member;
        }

        public override string ToString()
        {
            return $"[{Member}, {Score}]";
        }

        public override int GetHashCode()
        {
            return Member.GetHashCode();
        }

        internal ZSetEntry Increment(double increment)
        {
            return new ZSetEntry(Member, Score + increment);
        }
    }
}