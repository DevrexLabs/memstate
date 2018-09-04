using System.Threading.Tasks;

namespace Memstate
{
    public class NullMetricsProvider : MetricsProvider
    {
        public override IEngineMetrics GetEngineMetrics() => new NullEngineMetrics();
        public override IKernelMetrics GetKernelMetrics() => new NullKernelMetrics();
        public override Task<string> Report()
        {
            return Task.FromResult("No metrics provider");
        }
    }
}