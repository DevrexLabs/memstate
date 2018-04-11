using NUnit.Framework;

namespace Memstate.Test.Models.Redis
{
    [TestFixture]
    public class KeyTests : RedisTestBase
    {
        [Test]
        public void No_keys_after_clear()
        {
            Target.Set("key", "value");
            Target.Set("key2", "value");

            Target.Clear();
            
            Assert.AreEqual(0, Target.KeyCount());
            Assert.IsEmpty(Target.Keys());
        }

        [Test]
        public void Existing_key_exists()
        {
            Target.Set("key", "value");
            
            Assert.IsTrue(Target.Exists("key"));
        }

        [Test]
        public void Removed_key_does_not_exist()
        {
            Target.Set("key", "value");

            Target.Delete("key");
            
            Assert.IsFalse(Target.Exists("key"));
        }

        [Test]
        public void Delete_returns_number_of_keys_deleted()
        {
            Target.Set("number", "42");
            Target.Set("name", "ringnes");

            var actual = Target.Delete("number", "name");
            
            Assert.AreEqual(2, actual);
        }

        [Test]
        public void Random_key_null_no_keys_exists()
        {
            var actual = Target.RandomKey();
            
            Assert.IsNull(actual);
        }
    }
}