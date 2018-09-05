namespace Memstate.Docs.GettingStarted.QuickStart.Commands
{
    public class InitCustomer : Command<LoyaltyDB, Customer>
    {
        // we have an empty default constructor here because
        // we're using the NewtonSoft.Json serializer that needs
        // commands to have public empty constructors.

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