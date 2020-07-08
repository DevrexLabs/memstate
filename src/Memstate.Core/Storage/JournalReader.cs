using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate
{
    public abstract class JournalReader : IJournalReader
    {
        /// <inheritdoc />
        public Task Subscribe(long first, Action<JournalRecord> recordHandler, CancellationToken cancellationToken)
            => Subscribe(first, long.MaxValue, recordHandler, cancellationToken);

        /// <inheritdoc />
        public Task Subscribe(Action<JournalRecord> recordHandler, CancellationToken cancellationToken)
            => Subscribe(0, recordHandler, cancellationToken);


        /// <summary>
        /// <inheritdoc/>
        /// <remarks>
        /// Implementation that calls ReadRecords repeatedly until cancellation
        /// is requested or the last record is reached
        /// </remarks>
        /// </summary>
        /// <param name="first">record number of the first record to receive</param>
        /// <param name="last">record number of the last record to receive</param>
        /// <param name="recordHandler"></param>
        /// <param name="cancellationToken">token to stop the task </param>
        /// <returns>A task that completes after the last record or on cancellation </returns>
        public Task Subscribe(long first, long last,
            Action<JournalRecord> recordHandler, CancellationToken cancellationToken)
        {
            bool isLast = false;

            bool CheckLast(JournalRecord record) => isLast = (record.RecordNumber == last);
            var nextRecordNumber = first;
            return Task.Run(() =>
            {
                while (!isLast && !cancellationToken.IsCancellationRequested)
                {
                    foreach (var record in ReadRecords(nextRecordNumber))
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        recordHandler.Invoke(record);
                        if (CheckLast(record)) break;
                        nextRecordNumber++;
                    }
                }
            }, cancellationToken);    
        }
        
        /// <summary>
        /// Override this method when creating a custom storage provider
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public abstract IEnumerable<JournalRecord> ReadRecords(long from);

        public abstract Task DisposeAsync();
    }
}