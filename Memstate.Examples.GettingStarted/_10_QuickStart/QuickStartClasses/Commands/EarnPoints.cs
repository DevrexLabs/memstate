namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses.Commands
{

    public class EarnPoints : Command<LoyaltyDB, Customer>
    {
        public EarnPoints()
        {
        }

        public EarnPoints(int id, int points)
        {
            ID = id;
            Points = points;
        }

        public int ID { get; }
        public int Points { get; }

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