using System.Collections;
using System.Collections.Generic;

namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{

    public class GetAllCustomersQuery : Query<LoyaltyDB, Dictionary<int, Customer>>
    {
        public GetAllCustomersQuery()
        {
        }

        // It is safe to return live customer objects, because customer is immutable.
        public override Dictionary<int, Customer> Execute(LoyaltyDB model)
        {
            var customers = new Dictionary<int, Customer>();
            foreach (var c in model.Customers)
                customers.Add(c.Key, c.Value);
            return customers;
        }
    }

}