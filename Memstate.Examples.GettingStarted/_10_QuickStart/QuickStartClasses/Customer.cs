using System;

namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{
    [Serializable]
    public class Customer
    {
        public Customer(CustomerID id, int loyaltyPointBalance) : this(id.ID, loyaltyPointBalance)
        {

        }

        public Customer(int id, int loyaltyPointBalance)
        {
            ID = new CustomerID(id);
            LoyaltyPointBalance = loyaltyPointBalance;
        }

        public CustomerID ID { get; }

        public int LoyaltyPointBalance { get; }

        public override string ToString() => $"{ID}, {LoyaltyPointBalance} points";
    }
}