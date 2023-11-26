using System.Collections.Generic;

namespace Memstate
{
    public delegate void CommandExecuted(Command command, bool isLocal, IEnumerable<Event> events);
}