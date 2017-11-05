namespace Memstate
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class NullJournalReader : IJournalReader
    {
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            yield break;
        }
    }
}