using System;
using System.Collections.Generic;

namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{
    [Serializable]
    public class CustomerModel
    {
        public CustomerModel()
        {
            for (int i = 1; i < 11; i++)
            {
                var c = new Customer(i, 0);
                Customers.Add(c.ID, c);
            }
        }

        public IDictionary<CustomerID, Customer> Customers { get; } = new Dictionary<CustomerID, Customer>();
    }
}