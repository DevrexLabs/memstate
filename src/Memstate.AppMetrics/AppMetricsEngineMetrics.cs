using System;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Timer;

namespace Memstate.AppMetrics
{
    public class AppMetricsEngineMetrics : IEngineMetrics
    {
        private readonly IMetricsRoot _mtx;

        public AppMetricsEngineMetrics(IMetricsRoot metricsRoot)
        {
            _mtx = metricsRoot;
        }

        public void QueryExecuted()
        {
            var options = new CounterOptions
            {
                Name = "QueriesExecuted",
                MeasurementUnit = Unit.Calls
            };

            _mtx.Measure.Counter.Increment(options);
        }

        public void QueryFailed()
        {
            var options = new CounterOptions
            {
                Name = "QueriesFailed",
                MeasurementUnit = Unit.Calls
            };

            _mtx.Measure.Counter.Increment(options);
        }

        public void CommandExecuted()
        {
            var options = new CounterOptions
            {
                Name = "CommandsExecuted",
                MeasurementUnit = Unit.Calls
            };

            _mtx.Measure.Counter.Increment(options);
        }

        public void CommandFailed()
        {
            var options = new CounterOptions
            {
                Name = "CommandsFailed",
                MeasurementUnit = Unit.Calls
            };

            _mtx.Measure.Counter.Increment(options);
        }

        public void PendingLocalCommands(int value)
        {
            var options = new GaugeOptions
            {
                Name = "PendingCommands",
                MeasurementUnit = Unit.Items
            };

            _mtx.Measure.Gauge.SetValue(options, value);
        }

        public IDisposable MeasureCommandExecution()
        {
            var options = new TimerOptions
            {
                Name = "CommandExecutionTime",
                DurationUnit = TimeUnit.Milliseconds,
                MeasurementUnit = Unit.Requests,
                RateUnit = TimeUnit.Seconds
            };

            return _mtx.Measure.Timer.Time(options);
        }

        public IDisposable MeasureQueryExecution()
        {
            var options = new TimerOptions
            {
                Name = "QueryExecutionTime",
                MeasurementUnit = Unit.Requests,
                DurationUnit = TimeUnit.Microseconds,
                RateUnit = TimeUnit.Seconds
            };

            return _mtx.Measure.Timer.Time(options);
        }
    }
}