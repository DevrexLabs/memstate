using System;
using System.Collections.Generic;

namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{
    [Serializable]
    public class LoyaltyDB
    {
        public LoyaltyDB()
        {
        }

        public IDictionary<CustomerID, Customer> Customers { get; } = new Dictionary<CustomerID, Customer>();
    }
}