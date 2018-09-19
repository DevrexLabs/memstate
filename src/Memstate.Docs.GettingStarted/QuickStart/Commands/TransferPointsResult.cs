namespace Memstate.Docs.GettingStarted.QuickStart.Commands
{
    public class TransferPointsResult
    {
        public TransferPointsResult(Customer sender, Customer recipient, int points)
        {
            Sender = sender;
            Recipient = recipient;
            Points = points;
        }

        public int Points { get; }
        public Customer Sender { get; }
        public Customer Recipient { get; }
        public override string ToString()
        {
            return $"Sent {Points} points. | Sender, {Sender} | Recipient, {Recipient}";
        }
    }

}