using System;
using Newtonsoft.Json;

namespace Memstate.Test.EventfulTestDomain
{
    public class User
    {
        private User()
        {
        }

        public User(string username)
        {
            Id = Guid.NewGuid();
            Username = username;
        }

        [JsonProperty]
        public Guid Id { get; private set; }

        [JsonProperty]
        public string Username { get; private set; }
    }
}