using System;
using App.Metrics;
using App.Metrics.Timer;

namespace Memstate.AppMetrics
{
    public class AppMetricsKernelMetrics : IKernelMetrics
    {
        private readonly IMetricsRoot _mtx;

        public AppMetricsKernelMetrics(IMetricsRoot metricsRoot)
        {
            _mtx = metricsRoot;
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

            return _mtx.Measure.Timer.Time(options);
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

            return _mtx.Measure.Timer.Time(options);
        }
    }
}