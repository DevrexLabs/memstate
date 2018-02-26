using System;
using System.Linq;

namespace Memstate.Docs.GettingStarted._10_QuickStart.QuickStartClasses.Queries
{
    [Serializable]
    public class Top10Customers : Query<LoyaltyDB, Customer[]>
    {
        public override Customer[] Execute(LoyaltyDB db) => db.Customers
            .OrderByDescending(c => c.Value.LoyaltyPointBalance)
            .Take(10).Select(c => c.Value).ToArray();
    }
}