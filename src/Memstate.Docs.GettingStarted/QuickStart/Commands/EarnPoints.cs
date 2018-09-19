namespace Memstate.Docs.GettingStarted.QuickStart.Commands
{
    public class EarnPoints : Command<LoyaltyDB, Customer>
    {
        public EarnPoints(int customerId, int points)
        {
            CustomerId = customerId;
            Points = points;
        }

        public int CustomerId { get; }
        public int Points { get; }

        // it is safe to return a live customer object linked to the Model because
        // (1) the class is serializable and a remote client will get a serialized copy
        // and (2) in this particular case Customer is immutable.
        // if you have mutable classes, then please rather return a view, e.g. CustomerBalance or CustomerView class 

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = model.Customers[CustomerId];
            var newPoints = customer.LoyaltyPointBalance + Points;
            var customerWithNewBalance = new Customer(CustomerId, newPoints);
            model.Customers[CustomerId] = customerWithNewBalance;
            return customerWithNewBalance;
        }
    }

}