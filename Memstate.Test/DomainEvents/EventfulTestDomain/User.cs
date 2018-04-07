using System;

namespace Memstate.Test.EventfulTestDomain
{
    public class User
    {
        public User(string username)
        {
            Id = Guid.NewGuid();
            Username = username;
        }

        public Guid Id { get; private set; }

        public string Username { get; private set; }
    }
}