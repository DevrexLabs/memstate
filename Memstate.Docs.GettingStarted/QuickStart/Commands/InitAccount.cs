namespace Memstate.Docs.GettingStarted.QuickStart.Commands
{

    public class InitAccount : Command<LedgerDB, Account>
    {
        public InitAccount()
        {
        }

        public InitAccount(int accountNumber, decimal openingBalance, char currency)
        {
            AccountNumber = accountNumber;
            OpeningBalance = openingBalance;
            Currency = currency;
        }

        public int AccountNumber { get; }
        public decimal OpeningBalance { get; }
        public char Currency { get; }

        public override Account Execute(LedgerDB model)
        {
            if (model.Accounts.ContainsKey(AccountNumber))
            {
                return model.Accounts[AccountNumber];
            } 
            var account = new Account(AccountNumber, OpeningBalance, Currency);
            model.Accounts[AccountNumber] = account;
            return account;
        }
    }

}