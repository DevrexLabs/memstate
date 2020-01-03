using System;
using System.Threading.Tasks;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreJournalWriter : IJournalWriter
    {
        public SqlStreamStoreJournalWriter(long nextRecordNumber)
        {
            throw new NotImplementedException();
        }

        public Task DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public void Send(Command command)
        {
            throw new NotImplementedException();
        }
    }
}