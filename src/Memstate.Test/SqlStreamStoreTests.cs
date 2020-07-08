using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            var config = Config.CreateDefault();
            config.UseSqlStreamStore(new InMemoryStreamStore());
            var provider = config.GetStorageProvider();
            Assert.IsInstanceOf<SqlStreamStoreProvider>(provider);
        }

        [Test]
        public void ConfigUsingSqlStreamStore()
        {
            var config = Config.CreateDefault();
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
            var config = Config.CreateDefault();
            var streamStore = new InMemoryStreamStore();
            var provider = new SqlStreamStoreProvider(config, streamStore);

            config.SetStorageProvider(provider);
            var resolvedProvider = config.GetStorageProvider();

            Assert.AreSame(provider, resolvedProvider);
        }

        //, Ignore("Failing due to a concurrency bug")
        [Test]
        public async Task Smoke()
        {
            var config = Config.CreateDefault();
            config.GetSettings<EngineSettings>().StreamName = "stream1";
            var streamStoreProvider = new SqlStreamStoreProvider(config, new InMemoryStreamStore());
            var writer = streamStoreProvider.CreateJournalWriter();
            foreach(var i in Enumerable.Range(1,101))
                await writer.Write(new Set<int>("key" + i, i));
            await writer.DisposeAsync();

            var reader = streamStoreProvider.CreateJournalReader();
            var records = new List<JournalRecord>(reader.ReadRecords().ToArray());
            Assert.AreEqual(101, records.Count);
            Assert.AreEqual("key1", ((Set<int>) records[0].Command).Key);

            records.Clear();
            await reader.Subscribe(0, 100, jr => records.Add(jr), new CancellationToken());

            Console.WriteLine("sub.Ready()");

            Assert.AreEqual(101, records.Count);
            Assert.AreEqual("key1", ((Set<int>) records[0].Command).Key);
        }
    }
}