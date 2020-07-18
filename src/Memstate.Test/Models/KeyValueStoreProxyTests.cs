using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memstate.Models;

namespace Memstate.Test.Models
{
    public class KeyValueStoreProxyTests
    {
        private IKeyValueStore<int> _keyValueStore;
        private Client<IKeyValueStore<int>> _client;

        [SetUp]
        public async Task Setup()
        {
            var engine = await Engine.Start<IKeyValueStore<int>>(new KeyValueStore<int>());
            _client = new LocalClient<IKeyValueStore<int>>(engine);
            _keyValueStore = _client.GetDispatchProxy();
        }

        [TearDown]
        public Task Teardown() => _client.DisposeAsync();

        [Test]
        public void Set_stores_value()
        {
            _keyValueStore.Set("KEY", 1);
            var node = _keyValueStore.Get("KEY");
            Assert.AreEqual(1, node.Value);
        }

        [Test]
        public void Set_new_key_yields_correct_version()
        {
            _keyValueStore.Set("KEY", 1);
            var node = _keyValueStore.Get("KEY");
            Assert.AreEqual(1, node.Version);
        }

        [Test]
        public void Update_bumps_version()
        {
            _keyValueStore.Set("KEY", 1);
            _keyValueStore.Set("KEY", 2);
            var node = _keyValueStore.Get("KEY");
            Assert.AreEqual(2, node.Version);
        }

        [Test]
        public void Remove_throws_when_key_not_exists()
        {
            Assert.Throws<KeyNotFoundException> (() =>
            {
                _keyValueStore.Remove("KEY");
            });
        }
    }
}