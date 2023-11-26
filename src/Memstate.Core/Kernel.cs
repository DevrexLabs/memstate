using System;
using System.Collections.Generic;
using System.Threading;
using Memstate.Logging;

namespace Memstate;

/// <summary>
/// Provides thread safe access to the in-memory object graph,
/// executes commands and queries
/// </summary>
internal class Kernel
{
    private readonly object _model;

    private readonly ReaderWriterLockSlim _lock;

    private readonly IKernelMetrics _metrics;

    internal Kernel(EngineSettings config, object model)
    {
        _metrics = Metrics.Provider.GetKernelMetrics();
        var logger = LogProvider.GetCurrentClassLogger();
        _model = model;
        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        logger.Info("Created Kernel");
    }

    internal (object Result, Event[] events) Execute(Command command)
    {
        using (_metrics.MeasureCommandExecution())
        {
            try
            {
                var events = new List<Event>();
                _lock.EnterWriteLock();
                command.EventRaised = events.Add;
                var result = command.ExecuteImpl(_model);
                command.EventRaised = null;
                return (result, events.ToArray());
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    internal object Execute(Query query)
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