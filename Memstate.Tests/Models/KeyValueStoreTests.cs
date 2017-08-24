using System;
using System.Collections.Generic;
using Memstate.Models.KeyValue;
using Xunit;

namespace Memstate.Tests.Models
{
    public class KeyValueStoreTests
    {
        private readonly KeyValueStore<string> _store = new KeyValueStore<string>();

        [Fact]
        public void Given_no_existing_data_When_getting_a_value_Should_throw_KeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => _store.Get("NON_EXISTING_KEY"));
        }

        [Fact]
        public void Given_key_exists_When_getting_a_value_Should_return_value()
        {
            _store.Set("EXISTING_KEY", "VALUE");

            var node = _store.Get("EXISTING_KEY");

            Assert.Equal("VALUE", node.Value);
        }

        [Fact]
        public void Given_no_existing_data_When_setting_a_value_Should_store_value()
        {
            _store.Set("EXISTING_KEY", "VALUE");

            var node = _store.Get("EXISTING_KEY");

            Assert.Equal("VALUE", node.Value);
        }

        [Fact]
        public void Given_key_was_removed_by_another_user_When_setting_a_value_Should_throw_mismatching_version_exception()
        {
            var version = _store.Set("EXISTING_KEY", "VALUE");

            _store.Remove("EXISTING_KEY");

            Assert.Throws<InvalidOperationException>(() => _store.Set("EXISTING_KEY", "VALUE", version));
        }

        [Fact]
        public void Given_key_was_changed_by_another_user_When_setting_a_value_Should_throw_mismatching_version_exception()
        {
            var version = _store.Set("EXISTING_KEY", "VALUE");

            _store.Set("EXISTING_KEY", "VALUE");

            Assert.Throws<InvalidOperationException>(() => _store.Set("EXISTING_KEY", "VALUE", version));
        }

        [Fact]
        public void Given_key_exists_When_setting_a_value_Should_update_version_and_value()
        {
            var version = _store.Set("EXISTING_KEY", "VALUE");

            var newVersion = _store.Set("EXISTING_KEY", "NEW_VALUE");

            var node = _store.Get("EXISTING_KEY");

            Assert.True(newVersion > version);
            Assert.Equal("NEW_VALUE", node.Value);
        }

        [Fact]
        public void Given_no_existing_data_When_removing_a_value_Should_throw_KeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => _store.Get("EXISTING_KEY"));
        }

        [Fact]
        public void Given_key_was_changed_by_another_user_When_removing_a_value_Should_throw_exception_on_mismatching_version()
        {
            var version = _store.Set("EXISTING_KEY", "VALUE");
            
            _store.Set("EXISTING_KEY", "VALUE");

            Assert.Throws<InvalidOperationException>(() => _store.Remove("EXISTING_KEY", version));
        }

        [Fact]
        public void Given_key_exists_When_removing_a_value_Should_remove_node()
        {
            _store.Set("EXISTING_KEY", "VALUE");
            
            _store.Remove("EXISTING_KEY");

            Assert.Throws<KeyNotFoundException>(() => _store.Get("EXISTING_KEY"));
        }
    }
}