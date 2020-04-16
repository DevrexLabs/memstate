using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Models.KeyValue;
using Memstate.SqlStreamStore;
using NUnit.Framework;
using SqlStreamStore;

namespace Memstate.Test
{
    [TestFixture]
    public class SqlStreamStoreTests
    {
        [Test]
        public void ConfigUsingSqlStreamStoreDefault()
        {
            var config = Config.Current;
            config.UseSqlStreamStore();
            var provider = config.GetStorageProvider();
            Assert.IsInstanceOf<SqlStreamStoreProvider>(provider);
        }

        [Test]
        public void ConfigUsingSqlStreamStore()
        {
            var config = Config.Current;
            var streamStore = new InMemoryStreamStore();
            config.UseSqlStreamStore(streamStore);

            var resolvedStore = config.Container.Resolve<IStreamStore>();
            Assert.AreSame(streamStore, resolvedStore);

            var provider = config.GetStorageProvider();
            Assert.IsInstanceOf<SqlStreamStoreProvider>(provider);
        }

        [Test]
        public void ConfigSetStorageProvider()
        {
            var config = Config.Current;
            var streamStore = new InMemoryStreamStore();
            var provider = new SqlStreamStoreProvider(streamStore);

            config.SetStorageProvider(provider);
            var resolvedProvider = config.GetStorageProvider();

            Assert.AreSame(provider, resolvedProvider);
        }

        [Test, Ignore("Failing due to a concurrency bug")]
        public async Task Smoke()
        {
            var config = Config.Current;
            config.GetSettings<EngineSettings>().StreamName = "stream1";
            var streamStoreProvider = new SqlStreamStoreProvider();
            var writer = streamStoreProvider.CreateJournalWriter(0);
            foreach(var i in Enumerable.Range(1,101))
                writer.Send(new Set<int>("key" + i, i));
            await writer.DisposeAsync();

            var reader = streamStoreProvider.CreateJournalReader();
            var records = new List<JournalRecord>(reader.GetRecords().ToArray());
            Assert.AreEqual(101, records.Count);
            Assert.AreEqual("key1", ((Set<int>) records[0].Command).Key);

            records.Clear();
            var sub = streamStoreProvider
                .CreateJournalSubscriptionSource()
                .Subscribe(0, jr => records.Add(jr));
            while (!sub.Ready()) await Task.Delay(TimeSpan.FromMilliseconds(50));
            Console.WriteLine("sub.Ready()");

            Assert.AreEqual(101, records.Count);
            Assert.AreEqual("key1", ((Set<int>) records[0].Command).Key);
        }
    }
}