using System.Collections.Generic;

namespace Memstate
{
    // TODO: Refactor signature.
    public delegate void CommandExecutedDelegate(JournalRecord journalRecord, bool isLocal, IEnumerable<Event> events);
}