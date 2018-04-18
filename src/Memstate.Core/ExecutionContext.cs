using System.Collections.Generic;

namespace Memstate
{
    public class ExecutionContext
    {
        /// <summary>
        /// The context of the currently executing command.
        /// </summary>
        /// <value>The current.</value>
        public static ExecutionContext Current { get; set; }

        /// <summary>
        /// Record number of the currently executing command
        /// </summary>
        /// <value>The record number.</value>
        public long RecordNumber { get; internal set; }

        /// <summary>
        /// Add an event to the context. Events added will be captured by the <c>Engine</c>
        /// and published upon command completion.
        /// Subscribe to events using <c>Client.SubscribeAsync</c>
        /// </summary>
        public void AddEvent(Event @event)
        {
            Events.Add(@event);
        }

        internal List<Event> Events { get; set; }

        internal void Reset(long recordNumber)
        {
            RecordNumber = recordNumber;
            Events.Clear();
        }

        public ExecutionContext(long recordNumber)
        {
            Events = new List<Event>();
            RecordNumber = recordNumber;
        }
    }
}