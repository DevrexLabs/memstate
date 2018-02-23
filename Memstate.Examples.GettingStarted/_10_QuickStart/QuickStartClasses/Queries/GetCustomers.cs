using System.Collections.Generic;

namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses.Queries
{

    public class GetCustomers : Query<LoyaltyDB, Dictionary<int, Customer>>
    {
        public GetCustomers(){ 
        }

        public GetCustomers(params int[] ids)
        {
            IDs = ids;
        }

        public int[] IDs { get; private set; }

        // It is safe to return live customer objects, because customer is immutable.
        public override Dictionary<int, Customer> Execute(LoyaltyDB model)
        {
            var customers = new Dictionary<int, Customer>();
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