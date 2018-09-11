using Memstate.Docs.GettingStarted.QuickStart;
using Memstate.Docs.GettingStarted.QuickStart.Commands;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Memstate.Docs.GettingStarted.Configuration
{
    public class MinimalConfigurationTests
    {
        [SetUp]
        [TearDown]
        public void Setup()
        {
            if(File.Exists("mestate.journal")) File.Delete("memstate.journal");
        }

        [Test]
        public async Task Most_compact_start_using_all_default_configurations()
        {
            var engine = await Engine.Start<LoyaltyDB>();
            Print(await engine.Execute(new InitCustomer(10, 10)));
            Print(await engine.Execute(new InitCustomer(20, 20)));
            Print(await engine.Execute(new TransferPoints(10, 20, 5)));
            await engine.DisposeAsync();

            // Produces the following output :)

            /*             
            Customer[10] balance 10 points.
            Customer[20] balance 20 points.
            Sent 5 points. | Sender, Customer[10] balance 5 points. | Recipient, Customer[20] balance 25 points.
            */
        }

        private void Print(object o)
        {
            Console.WriteLine(o.ToString());
        }
    }
}
