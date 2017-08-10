using System;
using System.Collections.Generic;

namespace Memstate.Core
{
    public interface IJournalReader : IDisposable
    {
        IEnumerable<JournalRecord> GetRecords(long fromRecord = 0);
    }
}