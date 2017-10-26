using System;
using System.Collections.Generic;

namespace Memstate.Postgresql
{
    public class PostgresCommandStore : IJournalWriter, IJournalReader, IJournalSubscriptionSource
    {
        public PostgresCommandStore(Config config)
        {
        }

        public void Send(Command command)
        {
            throw new NotImplementedException();
        }

        void IJournalWriter.Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        public IJournalSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            throw new NotImplementedException();
        }
    }
}