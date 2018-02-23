namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses.Commands
{

    public class SpendPoints : Command<LoyaltyDB, Customer>
    {
        public SpendPoints()
        {
        }

        public SpendPoints(int id, int points)
        {
            ID = id;
            Points = points;
        }

        public int ID { get; private set; }

        public int Points { get; private set; }

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = model.Customers[ID];
            var newPoints = customer.LoyaltyPointBalance - Points;
            var customerWithNewBalance = new Customer(ID, newPoints);
            model.Customers[ID] = customerWithNewBalance;
            // It is safe to return live customer objects, because customer is immutable.
            return customerWithNewBalance;
        }
    }

}