using System;
using App.Metrics;
using App.Metrics.Timer;

namespace Memstate
{
    public class KernelMetrics
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
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds,
                MeasurementUnit = Unit.Requests
            };

            return _settings.Metrics.Measure.Timer.Time(options);
        }

        public IDisposable MeasureCommandExecution()
        {
            var options = new TimerOptions
            {
                Name = "KernelCommandExecutionTime",
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds,
                MeasurementUnit = Unit.Requests
            };

            return _settings.Metrics.Measure.Timer.Time(options);
        }
    }
}