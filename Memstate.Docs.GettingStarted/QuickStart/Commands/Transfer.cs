using System;
using System.Collections.Generic;
using System.Text;

namespace Memstate.Docs.GettingStarted.QuickStart.Commands
{
    public class Transfer : Command<LedgerDB, (Account from, Account to)>
    {
        public int FromAccount { get; }
        public int ToAccount { get; }
        public decimal Amount { get; }
        public char Currency { get; }

        public Transfer(int fromAccount, int toAccount, decimal amount, char currency)
        {
            FromAccount = fromAccount;
            ToAccount = toAccount;
            Amount = amount;
            Currency = currency;
        }

        public override (Account from, Account to) Execute(LedgerDB model)
        {
            if (Amount < 0) throw new ArgumentOutOfRangeException(nameof(Amount), "amount cannot be negative");
            var accounts = model.Accounts;
            var from = accounts[FromAccount];
            var to = accounts[ToAccount];
            var fromBalance = from.Balance - Amount;
            var toBalance = to.Balance + Amount;
            var newFrom = from.CloneWithNewBalance(fromBalance);
            var newTo = to.CloneWithNewBalance(toBalance);
            
            // --- all work done, now just need to update the model 
            accounts[FromAccount] = newFrom;
            accounts[ToAccount] = newTo;

            return (newFrom, newTo);
        }
    }
}
