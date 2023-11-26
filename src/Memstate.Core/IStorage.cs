using System.Collections.Generic;
using System.Threading.Tasks;

namespace Memstate;

public interface IStorage : IAsyncDisposable
{
    IAsyncEnumerable<JournalRecord[]> ReadRecords(long from = 1);
    Task<JournalRecord> Append(Command command);
}