namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{

    public class InitCustomerCommand : Command<LoyaltyDB, Customer>
    {
        public InitCustomerCommand()
        {
        }

        public InitCustomerCommand(int id, int points)
        {
            ID = id;
            Points = points;
        }

        public int ID { get; private set; }
        public int Points { get; private set; }

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = new Customer(ID, Points);
            model.Customers[ID] = customer;
            return customer;
        }
    }

}