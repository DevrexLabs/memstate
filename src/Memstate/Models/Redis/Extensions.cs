using System;

namespace Memstate.Models.Redis
{
    public static class Extensions
    {
        public static bool Expire(this RedisModel model, string key, TimeSpan after)
        {
            var expires = DateTime.Now + after;

            return model.Expire(key, expires);
        }
    }
}