using System;
using System.Linq;
using Newtonsoft.Json;

namespace Memstate.Test.EventfulTestDomain
{
    public class Delete : Command<UsersModel>
    {
        public Delete(Guid userId)
        {
            UserId = userId;
        }
        
        [JsonProperty]
        public Guid UserId { get; private set; }
        
        public override void Execute(UsersModel model, Action<Event> raise)
        {
            var user = model.Users.Values.FirstOrDefault(u => u.Id == UserId);

            if (user == null)
            {
                return;
            }

            model.Users.Remove(user.Username);

            raise(new Deleted(user.Id));
        }
    }
}