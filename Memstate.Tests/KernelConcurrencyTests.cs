using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Memstate.Core.Tests
{
    public class KernelConcurrencyTests
    {
        class AccountModel : Dictionary<int, int> { }

        class AccountsSummed : Query<AccountModel, int>
        {
            public override int Execute(AccountModel model)
            {
                return model.Values.Sum();
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

        readonly AccountModel _bank;
        readonly Kernel _kernel;

        public KernelConcurrencyTests(ITestOutputHelper testOutputHelper)
        {
            var config = new Config();
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

        [Fact]
        public void Transactions_sum_up_to_zero()
        {
            var commandTask = Task.Run(() =>
            {
                foreach (var randomTransferCommand in RandomTransferCommands(100000))
                {
                    _kernel.Execute(randomTransferCommand);
                }
            });

            int totalSum = 0;

            var queryTask = Task.Run(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    var query = new AccountsSummed();
                    totalSum += (int) _kernel.Execute(query);
                }
            });
            Task.WaitAll(commandTask, queryTask);
            Assert.Equal(0, totalSum);
        }

    }
}
