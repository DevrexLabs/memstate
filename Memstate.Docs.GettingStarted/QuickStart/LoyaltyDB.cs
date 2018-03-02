using System;
using System.Collections.Generic;

namespace Memstate.Docs.GettingStarted.QuickStart
{
    [Serializable]
    public class LoyaltyDB
    {
        public IDictionary<int, Customer> Customers { get; } = new Dictionary<int, Customer>();
    }
}