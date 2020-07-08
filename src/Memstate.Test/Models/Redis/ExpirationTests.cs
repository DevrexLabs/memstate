using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Models.Redis;
using NUnit.Framework;

namespace Memstate.Test.Models.Redis
{
    [TestFixture]
    public class ExpirationTests
    {
        [Test]
        [Ignore("Expiration not yet implemented")]
        public void Expires()
        {
            const string key = "key";
            var target = new RedisModel();
            target.Set(key, "value");
            var expires = DateTime.Now;
            target.Expire(key, expires);
            Thread.Sleep(TimeSpan.FromMilliseconds(1));
            var keys = target.GetExpiredKeys();
            Assert.IsTrue(keys.Single() == key);
            var expected = target.Expires(key);
            Assert.IsTrue(expected.HasValue);
            Assert.AreEqual(expected.Value, expires);
        }

        [Test, Ignore("Expiration not implemented")]
        public void ExpiresMultiple()
        {
            var target = new RedisModel();
            var expires = DateTime.Now;

            //add 5 keys with expiring NOW
            var range = Enumerable.Range(1, 5).Select(n => n.ToString()).ToArray();
            foreach (var key in range)
            {
                target.Set(key, key);
                target.Expire(key, expires);
            }

            //wait a bit and they should all be reported as expired
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
            var expiredKeys = target.GetExpiredKeys();
            Assert.IsTrue(new HashSet<string>(expiredKeys).SetEquals(range));

            //check them individually
            foreach (var key in range)
            {
                var expected = target.Expires(key);
                Assert.IsTrue(expected.HasValue);
                Assert.AreEqual(expected.Value, expires);
            }

            //un-expire the first one and check again
            target.Persist(range[0]);
            range = range.Skip(1).ToArray();
            expiredKeys = target.GetExpiredKeys();
            Assert.IsTrue(new HashSet<string>(expiredKeys).SetEquals(range));

            //purge and there should be no expired keys
            target.PurgeExpired();
            expiredKeys = target.GetExpiredKeys();
            Assert.AreEqual(expiredKeys.Length, 0);

            //there should now be a single key in the store
            Assert.AreEqual(target.KeyCount(), 1);
            Assert.AreEqual("1", target.Get("1"));
        }

        [Test]
        [Ignore("Expiration not implemented")]
        public async Task PurgeTimer()
        {
            var config = Config.CreateDefault();
            config.FileSystem = new InMemoryFileSystem();

            var engine = await Engine.Start<IRedisModel>(config);
            var redis = new LocalClient<IRedisModel>(engine).GetDispatchProxy();

            var mre = new ManualResetEvent(false);

            engine.CommandExecuted += (record, isLocal, events) =>
            {
                if (record.Command is PurgeExpiredKeysCommand)
                {
                    mre.Set();
                }
            };

            const string key = "key";
            redis.Set(key, "1");
            redis.Set("key2", "2");
            var expires = DateTime.Now;
            redis.Expire(key, expires);

            var signaled = mre.WaitOne(TimeSpan.FromSeconds(5));
            Assert.IsTrue(signaled, "No PurgeExpiredKeysCommand within time limit 5s");

            Assert.AreEqual(redis.KeyCount(), 1);
            await engine.DisposeAsync();

            engine = await Engine.Start<IRedisModel>();
            redis = new LocalClient<IRedisModel>(engine).GetDispatchProxy();
            Assert.AreEqual(redis.KeyCount(), 1);
            await engine.DisposeAsync();
        }
    }
}