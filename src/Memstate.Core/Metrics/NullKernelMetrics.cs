using System;

namespace Memstate
{
    public class NullKernelMetrics : IKernelMetrics
    {
        public IDisposable MeasureQueryExecution() => NullDisposable.Instance;

        public IDisposable MeasureCommandExecution() => NullDisposable.Instance;
    }
}