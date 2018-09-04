using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Memstate.Test
{
    [TestFixture]
    public class KernelConcurrencyTests
    {
        class AccountModel : Dictionary<int, int> { }

        class AccountsSummed : Query<AccountModel, int>
        {
            public override int Execute(AccountModel db)
            {
                return db.Values.Sum();
            }
        }
        class AccountTransfer : Command<AccountModel>
        {
            readonly int FromAccount;
            readonly int ToAccount;
            readonly int Amount;

            public AccountTransfer(int from, int to, int amount)
            {
                FromAccount = from;
                ToAccount = to;
                Amount = amount;
            }

            public override void Execute(AccountModel model)
            {
                if (!model.ContainsKey(FromAccount)) model[FromAccount] = 0;
                if (!model.ContainsKey(ToAccount)) model[ToAccount] = 0;
                model[FromAccount] -= Amount;
                model[ToAccount] += Amount;
            }
        }

        AccountModel _bank;
        Kernel _kernel;

        [SetUp]
        public void Setup()
        {
            var config = new EngineSettings();
            _bank = new AccountModel();
            _kernel = new Kernel(config, _bank);
        }

        private IEnumerable<Command> RandomTransferCommands(int numCommands)
        {
            Random rnd = new Random();
            for (int i = 0; i < numCommands; i++)
            {
                var from = rnd.Next(100);
                var to = rnd.Next(100);
                var amount = rnd.Next(1000);
                Task.Delay(rnd.Next(1)).Wait();
                yield return new AccountTransfer(from,to,amount);
            }
        }

        [Test]
        public async Task Transactions_sum_up_to_zero()
        {
            const int NumTransactions = 100000;

            var commandTask = Task.Run(() =>
            {
                foreach (var randomTransferCommand in RandomTransferCommands(NumTransactions / 2))
                {
                    _kernel.Execute(randomTransferCommand);
                }
            });


            var queryTask = Task.Run(() =>
            {
                for (int i = 0; i < NumTransactions / 2; i++)
                {
                    var query = new AccountsSummed();
                    var sum =  _kernel.Execute(query);
                    //the sum at any given point in time should always be 0
                    Assert.AreEqual(0,sum);
                }
            });
            await Task.WhenAll(commandTask, queryTask);
        }

    }
}
