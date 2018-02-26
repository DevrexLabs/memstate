namespace Memstate.Docs.GettingStarted._10_QuickStart.QuickStartClasses.Commands
{

    public class InitCustomer : Command<LoyaltyDB, Customer>
    {
        public InitCustomer()
        {
        }

        public InitCustomer(int id, int points)
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