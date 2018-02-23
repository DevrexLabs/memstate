namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{

    public class InitCustomerCommand : Command<LoyaltyDB, Customer>
    {
        public InitCustomerCommand()
        {
        }

        public InitCustomerCommand(CustomerID id, int points)
        {
            ID = id;
            Points = points;
        }

        //[JsonProperty]
        public CustomerID ID { get; private set; }

        //[JsonProperty]
        public int Points { get; private set; }

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = new Customer(ID, Points);
            model.Customers[ID] = customer;
            return customer;
        }
    }

}