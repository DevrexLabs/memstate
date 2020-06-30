using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Metrics;

namespace Memstate.AppMetrics
{
    public class AppMetricsProvider : MetricsProvider
    {
        private readonly IMetricsRoot _mtx;

        public AppMetricsProvider(IMetricsRoot mtx)
        {
            _mtx = mtx;
        }

        public AppMetricsProvider(): this(DefaultConfig())
        {

        }

        private static IMetricsRoot DefaultConfig()
        {
            return new MetricsBuilder().OutputMetrics.AsJson().Build();
        }

        public override IEngineMetrics GetEngineMetrics()
        {
            return new AppMetricsEngineMetrics(_mtx);
        }

        public override IKernelMetrics GetKernelMetrics()
        {
            return new AppMetricsKernelMetrics(_mtx);
        }

        public override async Task<string> Report()
        {
            var snapshot = _mtx.Snapshot.Get();
            var formatter = _mtx.DefaultOutputMetricsFormatter;

            using (var stream = new MemoryStream())
            {
                await formatter.WriteAsync(stream, snapshot);
                var result = Encoding.UTF8.GetString(stream.ToArray());
                return result;
            }
        }
    }
}