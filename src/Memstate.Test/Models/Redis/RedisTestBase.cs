using Memstate.Models.Redis;
using NUnit.Framework;

namespace Memstate.Test.Models.Redis
{
    public abstract class RedisTestBase
    {
        protected RedisModel Target;
        
        [SetUp]
        public void SetUp()
        {
            Target = new RedisModel();
        }
    }
}