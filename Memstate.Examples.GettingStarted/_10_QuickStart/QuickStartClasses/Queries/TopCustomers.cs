using System.Collections.Generic;
using System.Linq;

namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses.Queries
{

    public class TopCustomers : Query<LoyaltyDB, Dictionary<int, Customer>>
    {
        public TopCustomers(){ 
        }

        public TopCustomers(int minimumPoints)
        {
            MinimumPoints = minimumPoints;
        }

        public int MinimumPoints { get; private set; }

        // It is safe to return live customer objects, because customer is immutable.
        public override Dictionary<int, Customer> Execute(LoyaltyDB model)
        {
            var topCustomers = model.Customers
                .Where(c => c.Value.LoyaltyPointBalance >= MinimumPoints)
                .ToDictionary(i => i.Key, i => i.Value);
            return topCustomers;
        }
}

}