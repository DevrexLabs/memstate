using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Models.Redis
{
    public partial class RedisModel
    {
        public bool ZAdd(string key, string member, double score)
        {
            return ZAdd(key, new KeyValuePair<string, double>(member, score)) == 1;
        }

        public int ZAdd(string key, params KeyValuePair<string, double>[] items)
        {
            var d = items.ToDictionary(item => item.Key, item => item.Value);

            return ZAdd(key, d);
        }

        public int ZAdd(string key, IDictionary<string, double> membersAndScores)
        {
            var sortedSet = GetSortedSet(key, create: true);

            var elementsAdded = membersAndScores.Count;

            foreach (var entry in membersAndScores.Select(ms => new ZSetEntry(ms.Key, ms.Value)))
            {
                if (sortedSet.Remove(entry))
                {
                    elementsAdded--;
                }

                sortedSet.Add(entry);
            }

            if (sortedSet.Count == 0)
            {
                _structures.Remove(key);
            }

            return elementsAdded;
        }

        public int ZAdd(string key, params string[] scoreAndMembersInterlaced)
        {
            try
            {
                var d = ToPairs(scoreAndMembersInterlaced).ToDictionary(p => p.Value, p => double.Parse(p.Key));

                return ZAdd(key, d);
            }
            catch (FormatException exception)
            {
                throw new InvalidOperationException("value is not a valid float", exception);
            }
        }

        public int ZCard(string key)
        {
            var sortedSet = GetSortedSet(key);

            return sortedSet?.Count ?? 0;
        }

        public int ZCount(string key, double min, double max)
        {
            var sortedSet = GetSortedSet(key);

            if (sortedSet == null)
            {
                return 0;
            }

            return sortedSet.SkipWhile(entry => entry.Score < min)
                .TakeWhile(entry => entry.Score <= max)
                .Count();
        }

        public double ZIncrementBy(string key, double increment, string member)
        {
            var sortedSet = GetSortedSet(key);

            var entry = sortedSet.SingleOrDefault(e => e.Member == member);

            if (entry != null)
            {
                sortedSet.Remove(entry);
            }
            else
            {
                entry = new ZSetEntry(member, 0);
            }

            entry = entry.Increment(increment);

            sortedSet.Add(entry);

            return entry.Score;
        }

        public int ZInterStore(string destination, string[] keys, double[] weights = null, AggregateType aggregateType = AggregateType.Sum)
        {
            var sets = keys.Select(k => GetSortedSet(k) ?? new SortedSet<ZSetEntry>()).ToArray();

            return sets.Any(s => s.Count == 0) ? 0 : ZSetOperationAndStoreImpl(destination, sets, weights, aggregateType, SetOperation.Intersection);
        }

        public string[] ZRange(string key, int start = 0, int stop = -1)
        {
            return ZRangeImpl(key, start, stop).Select(entry => entry.Member).ToArray();
        }

        public SortedSet<ZSetEntry> ZRangeWithScores(string key, int start = 0, int stop = -1)
        {
            return new SortedSet<ZSetEntry>(ZRangeImpl(key, start, stop));
        }

        public string[] ZRangeByScore(string key, double min, double max, int skip = 0, int take = int.MaxValue)
        {
            return ZRangeByScoreImpl(key, min, max, skip, take).Select(e => e.Member).ToArray();
        }

        public SortedSet<ZSetEntry> ZRangeByScoreWithScores(string key, double min, double max, int skip = 0, int take = int.MaxValue)
        {
            return new SortedSet<ZSetEntry>(ZRangeByScoreImpl(key, min, max, skip, take));
        }

        public int? ZRank(string key, string member)
        {
            var set = GetSortedSet(key);

            if (set == null)
            {
                return null;
            }

            var index = -1;

            foreach (var entry in set)
            {
                index++;

                if (entry.Member == member)
                {
                    return index;
                }
            }

            return null;
        }

        public int ZRemove(string key, params string[] members)
        {
            var set = GetSortedSet(key);

            return set?.RemoveWhere(e => members.Contains(e.Member)) ?? 0;
        }

        public int ZRemoveRangeByRank(string key, int first, int last)
        {
            var set = GetSortedSet(key);

            return set == null ? 0 : ZRemove(key, ZRange(key, first, last));
        }

        public int ZRemoveRangeByScore(string key, double min, double max)
        {
            var set = GetSortedSet(key);

            return set == null ? 0 : ZRangeByScoreImpl(key, min, max, 0, int.MaxValue).ToArray().Count(set.Remove);
        }

        public double? ZScore(string key, string member)
        {
            var set = GetSortedSet(key);

            return set?.Where(e => e.Member == member)
                .Select(e => (double?) e.Score)
                .SingleOrDefault();
        }

        public string[] ZReverseRange(string key, int start, int stop)
        {
            var set = GetSortedSet(key);

            if (set == null)
            {
                return new string[0];
            }

            var range = new Range(start, stop).Flip(set.Count);

            return ZRange(key, range.FirstIdx, range.LastIdx).Reverse().ToArray();
        }

        public SortedSet<ZSetEntry> ZReverseRangeWithScores(string key, int start = 0, int stop = 0)
        {
            var set = GetSortedSet(key);

            if (set == null)
            {
                return new SortedSet<ZSetEntry>();
            }

            var range = new Range(start, stop, set.Count).Flip(set.Count);

            return ZRangeWithScores(key, range.FirstIdx, range.LastIdx);
        }

        public string[] ZReverseRangeByScore(
            string key,
            double min = double.MinValue,
            double max = double.MaxValue,
            int skip = 0,
            int take = int.MaxValue)
        {
            return ZRangeByScore(key, min, max)
                .Reverse()
                .Skip(skip)
                .Take(take)
                .ToArray();
        }

        public SortedSet<ZSetEntry> ZReverseRangeByScoreWithScores(
            string key,
            double min = double.MinValue,
            double max = double.MaxValue,
            int skip = 0,
            int take = int.MaxValue)
        {
            return new SortedSet<ZSetEntry>(
                ZRangeByScoreWithScores(key, min, max)
                    .Reverse()
                    .Skip(skip)
                    .Take(take));
        }

        public int? ZReverseRank(string key, string member)
        {
            var rank = ZRank(key, member);

            return rank.HasValue ? (int?) (GetSortedSet(key).Count - rank.Value - 1) : null;
        }

        public int ZUnionStore(
            string destination,
            string[] keys,
            double[] weights = null,
            AggregateType aggregateType = AggregateType.Sum)
        {
            var sets = keys.Select(k => GetSortedSet(k) ?? new SortedSet<ZSetEntry>()).ToArray();

            return ZSetOperationAndStoreImpl(destination, sets, weights, aggregateType, SetOperation.Union);
        }

        private int ZSetOperationAndStoreImpl(
            string destination,
            SortedSet<ZSetEntry>[] sets,
            double[] weights,
            AggregateType aggregateType,
            SetOperation setOperation)
        {
            if (weights != null && weights.Length != sets.Length)
            {
                throw new ArgumentException("number of weights must correspond to number of keys", nameof(weights));
            }

            double WeightOf(int idx) => weights?[idx] ?? 1.0;

            double Aggregator(double a, double b) => aggregateType == AggregateType.Sum
                ? a + b
                : aggregateType == AggregateType.Max
                    ? Math.Max(a, b)
                    : Math.Min(a, b);

            var newSet = new SortedSet<ZSetEntry>(
                sets.SelectMany((set, idx) => set.Select(e => new ZSetEntry(e.Member, e.Score * WeightOf(idx))))
                    .GroupBy(e => e.Member)
                    .Where(g => setOperation == SetOperation.Union || g.Count() == sets.Length)
                    .Select(g => new ZSetEntry(g.Key, g.Select(e => e.Score).Aggregate(Aggregator)))
            );

            if (newSet.Count > 0)
            {
                _structures[destination] = newSet;
            }

            return newSet.Count;
        }

        private IEnumerable<ZSetEntry> ZRangeByScoreImpl(string key, double min, double max, int skip, int take)
        {
            var set = GetSortedSet(key);

            if (set == null)
            {
                return new ZSetEntry[0];
            }

            return set.SkipWhile(e => e.Score < min)
                .TakeWhile(e => e.Score <= max)
                .Skip(skip)
                .Take(take);
        }

        private IEnumerable<ZSetEntry> ZRangeImpl(string key, int start, int stop)
        {
            var sortedSet = GetSortedSet(key);

            if (sortedSet == null)
            {
                return Enumerable.Empty<ZSetEntry>();
            }

            var range = new Range(start, stop, sortedSet.Count);

            return sortedSet.Skip(range.FirstIdx).Take(range.Length);
        }

        private SortedSet<ZSetEntry> GetSortedSet(string key, bool create = false)
        {
            return As(key, create, () => new SortedSet<ZSetEntry>());
        }

        private enum SetOperation
        {
            Union,
            Intersection
        }
    }
}