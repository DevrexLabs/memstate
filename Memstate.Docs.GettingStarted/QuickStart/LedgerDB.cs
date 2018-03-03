using System;
using System.Collections.Generic;

namespace Memstate.Docs.GettingStarted.QuickStart
{
    [Serializable]
    public class LedgerDB
    {
        public IDictionary<int, Account> Accounts { get; } = new Dictionary<int, Account>();
    }
}