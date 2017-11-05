using System;
using System.Collections.Generic;

namespace Memstate
{
    public interface IJournalReader : IAsyncDisposable
    {
        IEnumerable<JournalRecord> GetRecords(long fromRecord = 0);
    }
}