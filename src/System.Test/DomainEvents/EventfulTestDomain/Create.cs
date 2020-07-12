using System;

namespace Memstate.Test.EventfulTestDomain
{
    [Serializable]
    public class Create : Command<UsersModel, User>
    {
        public Create(string username)
        {
            Username = username;
        }

        public string Username { get; set; }

        public override User Execute(UsersModel model)
        {
            if (model.Users.TryGetValue(Username, out var user))
            {
                return user;
            }

            user = new User(Username);

            model.Users.Add(Username, user);

            RaiseEvent(new Created(user.Id));

            return user;
        }
    }
}