using System.Collections.Generic;
using System.Linq;

namespace Memstate.Docs.GettingStarted.QuickStart.Queries
{
    public class GetAccounts : Query<LedgerDB, List<Account>>
    {
        public override List<Account> Execute(LedgerDB db) => db.Accounts.Select(a => a.Value).ToList();
    }
}
