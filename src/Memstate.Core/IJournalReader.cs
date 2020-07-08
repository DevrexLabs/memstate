using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Memstate
{
    public interface IJournalReader : IAsyncDisposable
    {
        /// <summary>
        /// Subscribe from a given record and forward until cancellation is requested
        /// </summary>
        /// <param name="first">record number of the first record to receive</param>
        /// <param name="recordHandler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>a task that completes after cancellation</returns>
        Task Subscribe(long first, 
            Action<JournalRecord> recordHandler, 
            CancellationToken cancellationToken );

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <param name="first">record number of the first record to receive</param>
        /// <param name="last">record number of the last record to receive</param>
        /// <param name="recordHandler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>a task that completes either after the last record or upon cancellation </returns>
        Task Subscribe(long first, long last,
            Action<JournalRecord> recordHandler, CancellationToken cancellationToken);

        /// <summary>
        /// Subscribe from the beginning of the stream and forward
        /// </summary>
        /// <param name="recordHandler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Subscribe( 
            Action<JournalRecord> recordHandler, 
            CancellationToken cancellationToken );
        
        /// <summary>
        /// Read records from a given record number and to the end of the stream
        /// </summary>
        /// <param name="from">record number of the first record to receive</param>
        /// <returns>A stream of JournalRecords</returns>
        IEnumerable<JournalRecord> ReadRecords(long from = 0);
    }
}