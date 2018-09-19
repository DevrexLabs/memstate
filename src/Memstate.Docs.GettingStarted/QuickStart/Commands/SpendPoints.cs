namespace Memstate.Docs.GettingStarted.QuickStart.Commands
{
    public class SpendPoints : Command<LoyaltyDB, Customer>
    {
        public SpendPoints(int customerId, int points)
        {
            CustomerId = customerId;
            Points = points;
        }

        public int CustomerId { get; }

        public int Points { get; }

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = model.Customers[CustomerId];
            var newPoints = customer.LoyaltyPointBalance - Points;
            var customerWithNewBalance = new Customer(CustomerId, newPoints);
            model.Customers[CustomerId] = customerWithNewBalance;
            // It is safe to return live customer objects, because customer is immutable.
            return customerWithNewBalance;
        }
    }

}