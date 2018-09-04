using System;
using System.Linq;

namespace Memstate.Test.EventfulTestDomain
{
    public class Delete : Command<UsersModel>
    {
        public Delete(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; private set; }
        
        public override void Execute(UsersModel model)
        {
            var user = model.Users.Values.FirstOrDefault(u => u.Id == UserId);

            if (user == null)
            {
                return;
            }

            model.Users.Remove(user.Username);

            RaiseEvent(new Deleted(user.Id));
        }
    }
}