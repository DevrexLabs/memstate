using System;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Models.Redis
{
    public partial class RedisModel
    {
        public bool HSet(string key, string field, string value)
        {
            var hash = GetHash(key, create: true);

            var existing = hash.ContainsKey(field);

            hash[field] = value;

            return existing;
        }

        public int HDelete(string key, params string[] fields)
        {
            var hash = GetHash(key);

            return hash == null ? 0 : fields.Count(hash.Remove);
        }

        public bool HExists(string key, string field)
        {
            var hash = GetHash(key);

            return hash != null && hash.ContainsKey(field);
        }

        public string HGet(string key, string field)
        {
            var hash = GetHash(key);

            if (hash == null)
            {
                return null;
            }

            hash.TryGetValue(field, out var result);

            return result;
        }

        public string[] HGetAll(string key)
        {
            var hash = GetHash(key);

            if (hash == null)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>(hash.Count * 2);

            foreach (var pair in hash)
            {
                result.Add(pair.Key);
                result.Add(pair.Value);
            }

            return result.ToArray();
        }

        public long HIncrementBy(string key, string field, long delta)
        {
            var newValue = 0 + delta;

            var hash = GetHash(key, create: true);

            if (hash.TryGetValue(key, out var value))
            {
                newValue = long.Parse(value);

                newValue += delta;
            }

            hash[field] = newValue.ToString();

            return newValue;
        }

        public int HLen(string key)
        {
            var hash = GetHash(key);

            return hash?.Count ?? 0;
        }

        public string[] HKeys(string key)
        {
            var hash = GetHash(key);

            return hash?.Keys.ToArray() ?? Array.Empty<string>();
        }

        public string[] HValues(string key)
        {
            var hash = GetHash(key);

            return hash?.Values.ToArray() ?? Array.Empty<string>();
        }

        private Dictionary<string, string> GetHash(string key, bool create = false)
        {
            return As(key, create, () => new Dictionary<string, string>());
        }
    }
}