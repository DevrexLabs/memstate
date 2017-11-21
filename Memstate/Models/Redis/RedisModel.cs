using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Memstate.Models.Redis
{
    public partial class RedisModel : IDisposable
    {
        private readonly Random _random = new Random();
        private TimeSpan _purgeInterval = TimeSpan.FromSeconds(1);
        private readonly SortedDictionary<string, object> _structures = new SortedDictionary<string, object>();
        private readonly SortedSet<Expiration> _expirations = new SortedSet<Expiration>();
        private readonly SortedDictionary<string, Expiration> _expirationKeys = new SortedDictionary<string, Expiration>();
        private readonly Timer _purgeTimer;
        private volatile bool _disposed;

        public RedisModel()
        {
            _purgeTimer = new Timer(
                state =>
                {
                    var expired = GetExpiredKeys();

                    if (expired.Any())
                    {
                        PurgeExpired();
                    }
                },
                null,
                TimeSpan.FromTicks(0),
                _purgeInterval);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _purgeTimer.Dispose();
        }

        public int Delete(params string[] keys)
        {
            foreach (var key in keys)
            {
                Persist(key);
            }

            return keys.Count(key => _structures.Remove(key));
        }

        public void Clear()
        {
            _expirations.Clear();
            _expirationKeys.Clear();
            _structures.Clear();
        }

        public int KeyCount()
        {
            return _structures.Count;
        }

        public bool Exists(string key)
        {
            return _structures.ContainsKey(key);
        }

        public bool Expire(string key, DateTime at)
        {
            if (!Exists(key))
            {
                return false;
            }

            var expiration = new Expiration(key, at);

            _expirations.Remove(expiration);
            _expirationKeys[key] = expiration;
            _expirations.Add(expiration);

            return true;
        }

        public KeyType Type(string key)
        {
            if (!_structures.TryGetValue(key, out var value))
            {
                return KeyType.None;
            }

            switch (value)
            {
                case StringBuilder _:
                    return KeyType.String;

                case SortedSet<ZSetEntry> _:
                    return KeyType.SortedSet;

                case Dictionary<string, string> _:
                    return KeyType.Hash;

                case List<string> _:
                    return KeyType.List;

                case HashSet<string> _:
                    return KeyType.Set;

                case BitArray _:
                    return KeyType.BitSet;

                default:
                    return KeyType.None;
            }
        }

        public int Append(string key, string value)
        {
            var builder = GetStringBuilder(key, create: true);

            return builder.Append(value).Length;
        }

        public void Set(string key, string value)
        {
            Persist(key);

            _structures[key] = new StringBuilder(value);
        }

        public bool SetUnlessExists(string key, string value)
        {
            if (Exists(key))
            {
                return false;
            }

            Set(key, value);

            return true;
        }

        public string Get(string key)
        {
            var builder = GetStringBuilder(key);

            return builder != null ? builder.ToString() : null;
        }

        public string GetRange(string key, int start, int end)
        {
            var builder = GetStringBuilder(key);

            var range = new Range(start, end, builder.Length);

            if (range.FirstIdx >= builder.Length)
            {
                return string.Empty;
            }

            var lastIdx = Math.Min(range.LastIdx, builder.Length - 1);
            var length = lastIdx - range.FirstIdx + 1;

            if (length <= 0)
            {
                return string.Empty;
            }

            var result = new char[length];

            builder.CopyTo(range.FirstIdx, result, 0, length);

            return new string(result);
        }

        public int BitCount(string key, int firstBit = 0, int lastBit = int.MaxValue)
        {
            var count = 0;

            var bits = GetBitArray(key);

            if (bits == null)
            {
                return count;
            }

            var range = new Range(firstBit, lastBit, bits.Length);

            for (var i = range.FirstIdx; i <= range.LastIdx && i < bits.Length; i++)
            {
                if (bits.Get(i))
                {
                    count++;
                }
            }

            return count;
        }

        public bool GetBit(string key, int offset)
        {
            var bits = GetBitArray(key);

            return bits != null && bits.Length > offset && bits.Get(offset);
        }

        public bool SetBit(string key, int index, bool value = true)
        {
            if (index < 0)
            {
                // TODO: OrigoDB used CommandAbortedException
                throw new ArgumentException($"must be greater than 0, was {index}", nameof(index));
            }

            var bits = GetBitArray(key, create: true);

            if (bits.Length <= index)
            {
                bits.Length = index + 1;
            }

            var oldValue = bits.Get(index);

            bits.Set(index, value);

            return oldValue;
        }

        public int BitOp(BitOperator op, string key, params string[] sourceKeys)
        {
            var sources = sourceKeys.Select(k => GetBitArray(k)).ToList();

            if (sources.Any(x => x == null))
            {
                // TODO: OrigoDB used CommandAbortedException
                throw new ArgumentException("one or more source keys do not exists", nameof(sourceKeys));
            }

            if (op == BitOperator.Not && sources.Count > 1)
            {
                // TODO: OrigoDB used CommandAbortedException
                throw new ArgumentException("BitOperator.Not requires a single source", nameof(sourceKeys));
            }

            if (op != BitOperator.Not && sources.Count < 2)
            {
                // TODO: OrigoDB used CommandAbortedException
                throw new ArgumentException("BitOperator requires at least 2 source sets");
            }

            // Ensure all sources is of the same length
            var maxLength = sources.Max(s => s.Length);

            sources.ForEach(s => s.Length = maxLength);

            var result = new BitArray(sources[0]);

            if (op == BitOperator.Not)
            {
                result = result.Not();
            }
            else
            {
                foreach (var bits in sources.Skip(1))
                {
                    switch (op)
                    {
                        case BitOperator.And:
                            result.And(bits);
                            break;

                        case BitOperator.Or:
                            result.Or(bits);
                            break;

                        case BitOperator.Xor:
                            result.Xor(bits);
                            break;
                    }
                }
            }

            _structures[key] = result;

            return result.Length;
        }

        public int BitPos(string key, bool value, int firstBit = 0, int lastBit = int.MaxValue)
        {
            var bits = GetBitArray(key);

            if (bits == null)
            {
                return -1;
            }

            for (var i = firstBit; i < lastBit && i < bits.Length; i++)
            {
                if (bits.Get(i) == value)
                {
                    return i;
                }
            }

            return -1;
        }

        public string GetSet(string key, string value)
        {
            var builder = GetStringBuilder(key, create: true);

            var oldValue = builder.ToString();

            builder.Clear().Append(value);

            return oldValue;
        }

        public long DecrementBy(string key, long delta)
        {
            var decrementedValue = 0 - delta;

            var builder = GetStringBuilder(key);

            if (builder == null)
            {
                _structures[key] = new StringBuilder().Append(decrementedValue);
            }
            else
            {
                decrementedValue = long.Parse(builder.ToString()) - delta;

                builder.Clear();
                builder.Append(decrementedValue);
            }

            return decrementedValue;
        }

        public long Decrement(string key)
        {
            return DecrementBy(key, 1);
        }

        public long Increment(string key)
        {
            return DecrementBy(key, -1);
        }

        public long IncrementBy(string key, long delta)
        {
            return DecrementBy(key, -delta);
        }

        public string[] Keys(string regex = "^.*$")
        {
            return _structures.Keys.Where(k => Regex.IsMatch(k, regex)).ToArray();
        }

        public string[] MGet(params string[] keys)
        {
            return keys.Select(key => GetStringBuilder(key)).Select(builder => builder?.ToString()).ToArray();
        }

        public void MSet(params string[] keyValuePairs)
        {
            foreach (var pair in ToPairs(keyValuePairs))
            {
                Set(pair.Key, pair.Value);
            }
        }

        public void HMSet(string keys, params string[] keyValuePairs)
        {
            foreach (var pair in ToPairs(keyValuePairs))
            {
                HSet(keys, pair.Key, pair.Value);
            }
        }

        public string[] HMGet(string key, params string[] fields)
        {
            var hash = GetHash(key);

            if (hash == null)
            {
                return new string[fields.Length];
            }

            return fields.Select(
                    field =>
                    {
                        hash.TryGetValue(field, out var value);

                        return value;
                    })
                .ToArray();
        }

        public int StrLength(string key)
        {
            switch (Type(key))
            {
                case KeyType.String:
                    return GetStringBuilder(key).Length;
                case KeyType.BitSet:
                    return GetBitArray(key).Length;
                case KeyType.None:
                    return 0;
                default:
                    throw new ArgumentException("Key is neither String or BitSet", nameof(key));
            }
        }

        public string RandomKey()
        {
            return _structures.Count == 0 ? null : _structures.Skip(_random.Next(_structures.Count)).Select(x => x.Key).First();
        }

        public void Rename(string key, string newKey)
        {
            if (key == newKey)
            {
                throw new ArgumentException("Cannot be the same as key", nameof(newKey));
            }

            if (!_structures.ContainsKey(key))
            {
                throw new ArgumentException("No such key", nameof(key));
            }

            _structures[newKey] = _structures[key];

            _structures.Remove(key);
        }

        public bool Persist(string key)
        {
            if (!_expirationKeys.TryGetValue(key, out var expiration))
            {
                return false;
            }

            _expirations.Remove(expiration);
            _expirationKeys.Remove(key);

            return true;
        }

        public int PurgeExpired()
        {
            return Delete(GetExpiredKeys());
        }

        public DateTime? Expires(string key)
        {
            return _expirationKeys.TryGetValue(key, out var expiration) ? (DateTime?) expiration.Expires : null;
        }

        public string[] GetExpiredKeys()
        {
            // TODO: OrigoDB relied on getting time from the current execution context. This doesn't exist in Memstate
            // yet.
            var currentTime = DateTime.Now;

            return _expirations.TakeWhile(expiration => currentTime > expiration.Expires)
                .Select(expiration => expiration.Key)
                .ToArray();
        }

        private static IEnumerable<KeyValuePair<string, string>> ToPairs(string[] interlaced)
        {
            if (interlaced.Length % 2 != 0)
            {
                throw new ArgumentException("Odd number of arguments to MSet/HMSet", nameof(interlaced));
            }

            for (var i = 0; i < interlaced.Length; i += 2)
            {
                yield return new KeyValuePair<string, string>(interlaced[i], interlaced[i + 1]);
            }
        }

        private StringBuilder GetStringBuilder(string key, bool create = false)
        {
            return As(key, create, () => new StringBuilder());
        }

        private BitArray GetBitArray(string key, bool create = false)
        {
            return As(key, create, () => new BitArray(0));
        }

        private T As<T>(string key, bool create, Func<T> constructor) where T : class
        {
            var result = GetStructure<T>(key);

            if (result == null && create)
            {
                _structures[key] = result = constructor.Invoke();
            }

            return result;
        }

        private T GetStructure<T>(string key) where T : class
        {
            if (!_structures.TryGetValue(key, out var val))
            {
                return null;
            }

            if (val is T structure)
            {
                return structure;
            }

            throw new InvalidCastException("WRONGTYPE Operation against a key holding the wrong kind of value");
        }

        public enum KeyType
        {
            None,
            String,
            List,
            Hash,
            Set,
            SortedSet,
            GeoSet,
            BitSet
        }

        public enum AggregateType
        {
            Sum,
            Min,
            Max
        }

        internal class Expiration : IComparable<Expiration>
        {
            public readonly string Key;
            public readonly DateTime Expires;

            public Expiration(string key, DateTime expires)
            {
                Key = key;
                Expires = expires;
            }

            public int CompareTo(Expiration other)
            {
                if (Expires > other.Expires)
                {
                    return 1;
                }

                if (Expires < other.Expires)
                {
                    return -1;
                }

                return string.Compare(Key, other.Key, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return obj is Expiration expiration && expiration.Key == Key;
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }
    }
}