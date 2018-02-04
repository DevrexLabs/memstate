using System.Collections;
using System.Collections.Generic;

namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{

    public class GetCustomersQuery : Query<LoyaltyDB, Dictionary<CustomerID, Customer>>
    {
        public GetCustomersQuery(){ 
        }

        public GetCustomersQuery(params CustomerID[] ids)
        {
            IDs = ids;
        }

        public CustomerID[] IDs { get; private set; }

        // It is safe to return live customer objects, because customer is immutable.
        public override Dictionary<CustomerID, Customer> Execute(LoyaltyDB model)
        {
            var customers = new Dictionary<CustomerID, Customer>();
            foreach (var id in IDs)
            {
                if (model.Customers.ContainsKey(id))
                {
                    customers[id] = model.Customers[id];
                }
            }
            return customers;
        }
}

}