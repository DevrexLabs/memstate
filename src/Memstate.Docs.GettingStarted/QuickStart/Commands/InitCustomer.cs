namespace Memstate.Docs.GettingStarted.QuickStart.Commands
{
    public class InitCustomer : Command<LoyaltyDB, Customer>
    {
        public InitCustomer(int customerId, int points)
        {
            CustomerId = customerId;
            Points = points;
        }

        public int CustomerId { get; }

        public int Points { get; }

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = new Customer(CustomerId, Points);
            model.Customers[CustomerId] = customer;
            return customer;
        }
    }

}