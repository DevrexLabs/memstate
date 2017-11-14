using System;
using System.Threading;
using App.Metrics;
using App.Metrics.Timer;
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
        private readonly ILogger _logger;
        private readonly MemstateSettings _config;

        public Kernel(MemstateSettings config, object model, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            _logger = config.CreateLogger<Kernel>();
            _config = config;
            _model = model;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _logger.LogInformation("Created Kernel");
        }

        public Guid Id { get; }

        public object Execute(Command command)
        {
            var timerOptions = new TimerOptions
            {
                Name = "KernelCommandExecutionTime",
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds,
                MeasurementUnit = Unit.Requests,
                Tags = new MetricTags(new[] {"Kernel"}, new[] {Id.ToString()})
            };

            using (_config.Metrics.Measure.Timer.Time(timerOptions))
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
            var timerOptions = new TimerOptions
            {
                Name = "KernelQueryExecutionTime",
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds,
                MeasurementUnit = Unit.Requests,
                Tags = new MetricTags(new[] {"Kernel"}, new[] {Id.ToString()})
            };

            using (_config.Metrics.Measure.Timer.Time(timerOptions))
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