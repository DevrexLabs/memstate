using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Memstate
{
    /// <summary>
    /// Provides thread safe access to the in-memory object graph,
    /// executes commands and queries
    /// </summary>
    public class Kernel
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

                    using (new AttachedEventHandler(_model, eventHandler))
                    {
                        return command.ExecuteImpl(_model, eventHandler);
                    }
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

        private class AttachedEventHandler : IDisposable
        {
            private readonly IDomainEventSource _model;

            private readonly Action<Event> _handler;

            public AttachedEventHandler(object model, Action<Event> handler)
            {
                if (!(model is IDomainEventSource))
                {
                    return;
                }

                _model = (IDomainEventSource) model;

                _handler = handler;

                _model.EventRaised += handler;
            }

            public void Dispose()
            {
                if (_model == null)
                {
                    return;
                }

                _model.EventRaised -= _handler;
            }
        }
    }
}