using System;
using System.Collections.Generic;

namespace Memstate.Models
{
    public class KeyValueStore<T> : IKeyValueStore<T>
    {
        private readonly SortedDictionary<string, Node> _store = new SortedDictionary<string, Node>();

        public Node Get(string key)
        {
            if (!_store.TryGetValue(key, out var node))
            {
                throw new KeyNotFoundException($"No such key [{key}]");
            }

            return node;
        }

        public int Set(string key, T value, int? expectedVersion = null)
        {
            if (!_store.TryGetValue(key, out var node))
            {
                node = new Node();

                node.ExpectVersion(expectedVersion);

                _store[key] = node;
            }
            else
            {
                node.ExpectVersion(expectedVersion);
            }

            return node.BumpAndSet(value);
        }

        public void Remove(string key, int? expectedVersion = null)
        {
            if (!_store.TryGetValue(key, out var node))
            {
                throw new KeyNotFoundException($"Key [{key}]");
            }

            node.ExpectVersion(expectedVersion);

            _store.Remove(key);
        }

        public int Count()
        {
            return _store.Count;
        }

        public class Node
        {
            public int Version { get; private set; }

            public T Value { get; private set; }

            internal int BumpAndSet(T value)
            {
                Value = value;

                return ++Version;
            }

            internal Node()
            {
            }

            public void ExpectVersion(int? version)
            {
                if (version.HasValue && Version != version.Value)
                {
                    throw new InvalidOperationException("Version mismatch");
                    //throw new CommandAbortedException("Version mismatch");
                }
            }
        }
    }
}