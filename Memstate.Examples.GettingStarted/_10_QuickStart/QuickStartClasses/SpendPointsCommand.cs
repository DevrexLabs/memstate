namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{

    public class SpendPointsCommand : Command<LoyaltyDB, Customer>
    {
        public SpendPointsCommand()
        {
        }

        public SpendPointsCommand(CustomerID id, int points)
        {
            ID = id;
            Points = points;
        }

        //[JsonProperty]
        public CustomerID ID { get; private set; }

        //[JsonProperty]
        public int Points { get; private set; }

        public Customer Execute(LoyaltyDB model)
        {
            var customer = model.Customers[ID];
            var newPoints = customer.LoyaltyPointBalance - Points;
            customer = new Customer(ID, newPoints);
            return customer;
        }
    }

}