using System.Collections.Generic;

namespace Memstate
{
    public delegate void CommandExecuted(JournalRecord journalRecord, bool isLocal, IEnumerable<Event> events);
}