using System.Collections.Generic;

namespace Memstate.Docs.GettingStarted.QuickStart.Queries
{
    public class GetCustomers : Query<2LoyaltyDB, IDictionary<int, Customer>>
    {
        public override IDictionary<int, Customer> Execute(LoyaltyDB db) => new Dictionary<int, Customer>(db.Customers);
    }
}