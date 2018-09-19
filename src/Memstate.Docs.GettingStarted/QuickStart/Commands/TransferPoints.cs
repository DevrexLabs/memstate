namespace Memstate.Docs.GettingStarted.QuickStart.Commands
{

    public class TransferPoints : Command<LoyaltyDB, TransferPointsResult>
    {
        public TransferPoints(int senderId, int recieverId, int points)
        {
            SenderId = senderId;
            RecieverId = recieverId;
            Points = points;
        }

        public int RecieverId { get; set;  }

        public int SenderId { get; set;  }

        public int Points { get; set; }

        // it is safe to return actual references to Customer objects, because they are immutable.
        public override TransferPointsResult Execute(LoyaltyDB model)
        {
            var sender = model.Customers[SenderId];
            var receiver = model.Customers[RecieverId];
            var newSender = new Customer(sender.ID, sender.LoyaltyPointBalance - Points);
            var newReceiver = new Customer(receiver.ID, receiver.LoyaltyPointBalance + Points);
            model.Customers[SenderId] = newSender;
            model.Customers[RecieverId] = newReceiver;
            return new TransferPointsResult(newSender, newReceiver, Points);
        }
    }

}