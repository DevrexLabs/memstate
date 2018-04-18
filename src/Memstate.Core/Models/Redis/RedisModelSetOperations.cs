using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Models.Redis
{
    public partial class RedisModel
    {
        public int SAdd(string key, params string[] values)
        {
            var set = GetSet(key, create: true);

            return values.Count(set.Add);
        }

        public int SCard(string key)
        {
            var set = GetSet(key);

            return set?.Count ?? 0;
        }

        public string[] SDiff(string key, params string[] setsToSubtract)
        {
            IEnumerable<string> set = GetSet(key);

            if (set == null)
            {
                return Array.Empty<string>();
            }

            var empty = new HashSet<string>();

            return setsToSubtract.Aggregate(set, (current, item) => current.Except(GetSet(item) ?? empty)).ToArray();
        }

        public int SDiffStore(string destination, string key, params string[] setsToSubtract)
        {
            var members = SDiff(key, setsToSubtract);

            if (members.Length > 0)
            {
                _structures[destination] = new HashSet<string>(members);
            }

            return members.Length;
        }

        public string[] SInter(string key, params string[] keys)
        {
            IEnumerable<string> set = GetSet(key);

            if (set == null)
            {
                return Array.Empty<string>();
            }

            var empty = new HashSet<string>();

            return keys.Aggregate(set, (current, item) => current.Intersect(GetSet(item) ?? empty)).ToArray();
        }

        public int SInterStore(string destination, string key, params string[] keys)
        {
            var members = SInter(key, keys);

            if (members.Length == 0)
            {
                return 0;
            }

            _structures[destination] = new HashSet<string>(members);

            return members.Length;
        }

        public bool SIsMember(string key, string value)
        {
            var set = GetSet(key);

            return set != null && set.Contains(value);
        }

        public string[] SMembers(string key)
        {
            return SInter(key);
        }

        public bool SMove(string sourceKey, string destinationKey, string value)
        {
            bool removed;

            var source = GetSet(sourceKey);

            if (source == null)
            {
                return false;
            }

            var destination = GetSet(destinationKey, create: true);

            removed = source.Remove(value);

            if (removed)
            {
                destination.Add(value);
            }

            if (destination.Count == 0)
            {
                _structures.Remove(destinationKey);
            }

            return removed;
        }

        public string SPop(string key)
        {
            var result = SRandMember(key);

            if (result.Length == 0)
            {
                return null;
            }

            GetSet(key).Remove(result[0]);

            return result[0];
        }

        public string[] SRandMember(string key, int count = -1)
        {
            var set = GetSet(key);

            if (set == null)
            {
                return null;
            }

            if (set.Count == 0 || count == 0)
            {
                return Array.Empty<string>();
            }

            var allowDuplicates = count < 0;

            if (allowDuplicates)
            {
                count = -count;
            }

            if (!allowDuplicates && count >= set.Count)
            {
                return set.ToArray();
            }

            var result = new Dictionary<int, string>();

            var randomIndicies = allowDuplicates ? (ICollection<int>) new List<int>(count) : new HashSet<int>();

            while (randomIndicies.Count < count)
            {
                randomIndicies.Add(_random.Next(count));
            }

            var index = 0;

            foreach (var member in set)
            {
                if (randomIndicies.Contains(index))
                {
                    result[index++] = member;
                }
            }

            return randomIndicies.Select(i => result[i]).ToArray();
        }

        public int SRemove(string key, params string[] members)
        {
            var set = GetSet(key);

            if (set == null || set.Count == 0)
            {
                return 0;
            }

            return members.Count(set.Remove);
        }

        public string[] SUnion(string key, params string[] keys)
        {
            IEnumerable<string> set = GetSet(key) ?? new HashSet<string>();

            set = keys.Select(k => GetSet(k) ?? new HashSet<string>())
                .Aggregate(set, (current, s) => current.Union(s));

            return set.ToArray();
        }

        public int SUnionStore(string destination, string key, params string[] keys)
        {
            var items = SUnion(key, keys);

            if (items.Length > 0)
            {
                _structures[destination] = new HashSet<string>(items);
            }

            return items.Length;
        }

        private HashSet<string> GetSet(string key, bool create = false)
        {
            return As(key, create, () => new HashSet<string>());
        }
    }
}