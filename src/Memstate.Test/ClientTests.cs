using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Models;
using Memstate.Models.KeyValue;
using NUnit.Framework;

namespace Memstate.Test
{
    public class ClientTests
    {
        [Test]
        public async Task HappyPath()
        {
            var config = Config.CreateDefault();
            // Create LocalClient wrapping a newly started engine
            var client = await Client.For<KeyValueStore<int>>(config);

            // execute commands
            await client.Execute(new Set<int>("a", 42));
            var resultingVersion = await client.Execute(new Set<int>("a", 43));
            Assert.AreEqual(2, resultingVersion);

            //query
            var record = await client.Execute(new Get<int>("a"));
            Assert.AreEqual(2, record.Version);
            Assert.AreEqual(43, record.Value);

            //Create a new client, it should connect to the same engine
            var client2 = await Client.For<KeyValueStore<int>>(config);
            var record2 = await client2.Execute(new Get<int>("a"));
            Assert.AreEqual(record2.Value, record.Value);
            Assert.AreEqual(record2.Version, record.Version);

            await client.DisposeAsync();
        }
    }
}