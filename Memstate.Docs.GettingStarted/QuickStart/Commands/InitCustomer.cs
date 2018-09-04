using Newtonsoft.Json;

namespace Memstate.Docs.GettingStarted.QuickStart.Commands
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

        [JsonProperty("ID")]
        public int ID { get; private set; }

        [JsonProperty("Points")]
        public int Points { get; private set; }

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = new Customer(ID, Points);
            model.Customers[ID] = customer;
            return customer;
        }
    }

}