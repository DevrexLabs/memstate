using System;

namespace Memstate.Models.Redis
{
    public class PurgeExpiredKeysCommand : Command<RedisModel>
    {
        public override void Execute(RedisModel model, Action<Event> eventHandler)
        {
            model.PurgeExpired();
        }
    }
}