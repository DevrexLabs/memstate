using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Models.KeyValue;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public class SqlStreamStoreTests
    {
        [Test]
        public async Task Smoke()
        {
            var config = Config.Current;
            config.GetSettings<EngineSettings>().StreamName = "stream1";
            var streamStoreProvider = new SqlStreamStore.SqlStreamStoreProvider(config);
            var writer = streamStoreProvider.CreateJournalWriter(0);
            foreach(var i in Enumerable.Range(1,100))
                writer.Send(new Set<int>("key" + i, i));
            await writer.DisposeAsync();

            var reader = streamStoreProvider.CreateJournalReader();
            var records = new List<JournalRecord>(reader.GetRecords().ToArray());
            Assert.Equals(100, records.Count);
            Assert.Equals("key1", ((Set<int>) records[0].Command).Key);
            
            records.Clear();
            var sub = streamStoreProvider.CreateJournalSubscriptionSource().Subscribe(0, jr => records.Add(jr));
            while (!sub.Ready()) await Task.Delay(TimeSpan.FromMilliseconds(50));

            Assert.Equals(100, records.Count);
            Assert.Equals("key1", ((Set<int>) records[0].Command).Key);
        }
    }
}