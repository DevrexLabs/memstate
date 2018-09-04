using System;

namespace Memstate
{
    public interface IKernelMetrics
    {
        IDisposable MeasureQueryExecution();
        IDisposable MeasureCommandExecution();
    }
}