using System.Collections.Generic;

namespace Memstate.Test.EventfulTestDomain
{
    public class UsersModel
    {
        public Dictionary<string, User> Users { get; } = new Dictionary<string, User>();
    }
}