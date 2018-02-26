using System;

namespace Memstate.Docs.GettingStarted._10_QuickStart.QuickStartClasses
{
    [Serializable]
    public class Customer
    {
        public Customer(int id, int loyaltyPointBalance)
        {
            ID = id;
            LoyaltyPointBalance = loyaltyPointBalance;
        }

        public int ID { get; }

        public int LoyaltyPointBalance { get; }

        public override string ToString() => $"{ID}, {LoyaltyPointBalance} points";
    }
}