using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memstate.Configuration;
using NUnit.Framework;

namespace Memstate.EngineTest
{
    [TestFixture]
    public class EngineExceptionTests
    {
        [Test]
        public async Task CanResumeAfterFailedCommand()
        {
            var config = Config.CreateDefault();
            config.SerializerName = Serializers.NewtonsoftJson;
            var settings = config.GetSettings<EngineSettings>();
            settings.WithRandomSuffixAppendedToStreamName();
            config.UseInMemoryFileSystem();
            var db = await Engine.Start<LoyaltyDB>(config);


                //Create a customer with id 1
                await db.Execute(new InitCustomer(1, 10));

                //try to make a transfer from customer 1 to 2, should fail because 
                var transfer = new TransferPoints(1, 2, 10);
                Exception caught = null;
                try
                {
                    await db.Execute(transfer);
                }
                catch (NotFoundException e)
                {
                    caught = e;
                }

                Assert.True(caught is NotFoundException);
                
                //add the customer with the missing id
                await db.Execute(new InitCustomer(2, 10));
                
                //try to make the transfer again, this time it should succeed
                await db.Execute(transfer);
                await db.DisposeAsync();
        }
    }
    
    public class LoyaltyDB
    {
        public IDictionary<long, Customer> Customers { get; } = new Dictionary<long, Customer>();
    }
    public class Customer
    {
        public Customer(long id, int loyaltyPointBalance)
        {
            CustomerId = id;
            LoyaltyPointBalance = loyaltyPointBalance;
        }
        public long CustomerId { get; }
        public int LoyaltyPointBalance { get; }
    }
    public class TransferPoints : Command<LoyaltyDB, TransferPointsResult>
    {
        /// <exception cref="NotFoundException">when sender or receiver ID not found.</exception>
        public TransferPoints(long senderId, long receiverId, int points)
        {
            SenderId = senderId;
            ReceiverId = receiverId;
            Points = points;
        }
        public long ReceiverId { get; set; }
        public long SenderId { get; set; }
        public int Points { get; set; }

        public override TransferPointsResult Execute(LoyaltyDB model)
        {
            if (!model.Customers.ContainsKey(SenderId) && !model.Customers.ContainsKey(ReceiverId)) throw new NotFoundException($"Sending customer [{SenderId}] not found, and recieving customer[{ ReceiverId }] not found.");
            if (!model.Customers.ContainsKey(SenderId)) throw new NotFoundException($"Sending customer [{SenderId}] not found.");
            if (!model.Customers.ContainsKey(ReceiverId)) throw new NotFoundException($"Recieving customer [{ReceiverId}] not found.");

            var sender = model.Customers[SenderId];
            var receiver = model.Customers[ReceiverId];
            var newSender = new Customer(sender.CustomerId, sender.LoyaltyPointBalance - Points);
            var newReceiver = new Customer(receiver.CustomerId, receiver.LoyaltyPointBalance + Points);

            model.Customers[SenderId] = newSender;
            model.Customers[ReceiverId] = newReceiver;
            // need to simulate slow action?? Thread.Sleep() eeeiurgh!!!
            return new TransferPointsResult(newSender, newReceiver, Points);
        }
    }
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message) : base(message){}
        public NotFoundException(string message, Exception innerException) : base(message, innerException){}
    }
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
    public class InitCustomer : Command<LoyaltyDB, Customer>
    {
        public InitCustomer(long customerId, int points)
        {
            CustomerId = customerId;
            Points = points;
        }

        public long CustomerId { get; }

        public int Points { get; }

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = new Customer(CustomerId, Points);
            model.Customers[CustomerId] = customer;
            return customer;
        }
    }
}