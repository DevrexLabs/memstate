using System;

namespace Memstate.Core
{
    public class JournalEntry
    {
        public readonly long SequenceNumber;
        public readonly DateTimeOffset Read;
        public readonly DateTimeOffset Written;
        public readonly Command Command;

        public TimeSpan Age => Read - Written;

        public JournalEntry(long sequenceNumber, DateTime written, Command command)
        {
            Read = DateTimeOffset.Now;
            SequenceNumber = sequenceNumber;
            Command = command;
            Written = written;
        }
    }
}