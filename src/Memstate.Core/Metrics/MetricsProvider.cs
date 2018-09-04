using System.Threading.Tasks;

namespace Memstate
{
    public abstract class MetricsProvider
    {
        public abstract IEngineMetrics GetEngineMetrics();
        public abstract IKernelMetrics GetKernelMetrics();
        public abstract Task<string> Report();
    }
}