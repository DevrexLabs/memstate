using System;
using App.Metrics;
using App.Metrics.Timer;

namespace Memstate
{
    internal class KernelMetrics
    {
        private readonly MemstateSettings _settings;

        public KernelMetrics(MemstateSettings settings)
        {
            _settings = settings;
        }

        public IDisposable MeasureQueryExecution()
        {
            var options = new TimerOptions
            {
                Name = "KernelQueryExecutionTime",
                DurationUnit = TimeUnit.Microseconds,
                MeasurementUnit = Unit.Requests,
                RateUnit = TimeUnit.Seconds
            };

            return _settings.Metrics.Measure.Timer.Time(options);
        }

        public IDisposable MeasureCommandExecution()
        {
            var options = new TimerOptions
            {
                Name = "KernelCommandExecutionTime",
                DurationUnit = TimeUnit.Milliseconds,
                MeasurementUnit = Unit.Requests,
                RateUnit = TimeUnit.Seconds
            };

            return _settings.Metrics.Measure.Timer.Time(options);
        }
    }
}