namespace Memstate.Docs.GettingStarted.QuickStart.Commands
{
    public class EarnPoints : Command<LoyaltyDB, Customer>
    {
        // Don't need an empty public constructor if we're using Wire serializer.

        public EarnPoints(int id, int points)
        {
            ID = id;
            Points = points;
        }

        public int ID { get; }
        public int Points { get; }

        // it is safe to return a live customer object linked to the Model because
        // (1) the class is serializable and a remote client will get a serialized copy
        // and (2) in this particular case Customer is immutable.
        // if you have mutable classes, then please rather return a view, e.g. CustomerBalance or CustomerView class 

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = model.Customers[ID];
            var newPoints = customer.LoyaltyPointBalance + Points;
            var customerWithNewBalance = new Customer(ID, newPoints);
            model.Customers[ID] = customerWithNewBalance;
            return customerWithNewBalance;
        }
    }

}