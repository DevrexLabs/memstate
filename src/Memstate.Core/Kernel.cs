using System;
using System.Threading;
using Memstate.Logging;

namespace Memstate
{
    /// <summary>
    /// Provides thread safe access to the in-memory object graph,
    /// executes commands and queries
    /// </summary>
    internal class Kernel
    {
        private readonly object _model;

        private readonly ReaderWriterLockSlim _lock;

        private readonly IKernelMetrics _metrics;

        public Kernel(MemstateSettings config, object model)
        {
            _metrics = Metrics.Provider.GetKernelMetrics();
            var logger = LogProvider.GetCurrentClassLogger();
            _model = model;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            logger.Info("Created Kernel");
        }

        public object Execute(Command command)
        {
            using (_metrics.MeasureCommandExecution())
            {
                try
                {
                    _lock.EnterWriteLock();
                    return command.ExecuteImpl(_model);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public object Execute(Query query)
        {
            using (_metrics.MeasureQueryExecution())
            {
                try
                {
                    _lock.EnterReadLock();
                    return query.ExecuteImpl(_model);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }
   }
}