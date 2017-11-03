namespace Memstate
{
    using System.Collections.Generic;

    public class NullJournalReader : IJournalReader
    {
        public void Dispose()
        {
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            yield break;
        }
    }
}