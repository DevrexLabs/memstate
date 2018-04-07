using System;
using System.Threading;
using Microsoft.Extensions.Logging;

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

        private readonly KernelMetrics _metrics;

        public Kernel(MemstateSettings config, object model)
        {
            _metrics = new KernelMetrics(config);
            var logger = config.CreateLogger<Kernel>();
            _model = model;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            logger.LogInformation("Created Kernel");
        }

        public object Execute(Command command, Action<Event> eventHandler)
        {
            using (_metrics.MeasureCommandExecution())
            {
                try
                {
                    _lock.EnterWriteLock();
                    command.EventRaised += eventHandler;
                    return command.ExecuteImpl(_model);
                }
                finally
                {
                    command.EventRaised -= eventHandler;
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