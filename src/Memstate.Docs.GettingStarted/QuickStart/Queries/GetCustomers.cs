﻿using System.Collections.Generic;

namespace Memstate.Docs.GettingStarted.QuickStart.Queries
{
    public class GetCustomers : Query<LoyaltyDB, IDictionary<int, Customer>>
    {
        public override IDictionary<int, Customer> Execute(LoyaltyDB db) => new Dictionary<int, Customer>(db.Customers);
    }
}