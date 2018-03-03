using System;

namespace Memstate.Docs.GettingStarted.QuickStart
{
    [Serializable]
    public class Account
    {
        public int AccountNumber { get; }
        public decimal Balance { get; }
        public char Currency { get; }

        public Account(int accountNumber, decimal balance, char currency)
        {
            AccountNumber = accountNumber;
            Balance = balance;
            Currency = currency;
        }

        public override string ToString()
        {
            return $"Account: {AccountNumber} - Balance {Currency}{Balance,0:0.00}";
        }
    }

    public static class AccountExtensions
    {
        public static Account CloneWithNewBalance(this Account account, decimal newBalance)
        {
            return new Account(account.AccountNumber, newBalance, account.Currency);
        }
    }

}