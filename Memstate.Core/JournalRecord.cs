using System;

namespace Memstate
{
    public class JournalRecord
    {
        /// <summary>
        /// Sequential id of the record, always starts at 0
        /// </summary>
        public readonly long RecordNumber;


        /// <summary>
        /// Point in time when the record was written to the journal
        /// </summary>
        public readonly DateTimeOffset Written;

        public readonly Command Command;

        public JournalRecord(long recordNumber, DateTime written, Command command)
        {
            RecordNumber = recordNumber;
            Command = command;
            Written = written;
        }
    }
}