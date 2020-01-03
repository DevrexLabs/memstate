using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamSourceJournalReader : IJournalReader
    {
        public Task DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            throw new NotImplementedException();
        }
    }
}