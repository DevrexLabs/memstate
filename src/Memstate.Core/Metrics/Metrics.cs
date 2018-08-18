using System;
using System.Threading.Tasks;

namespace Memstate
{
    public class Metrics
    {
        private const string AppMetricsProviderType = "Memstate.AppMetrics.AppMetricsProvider, Memstate.AppMetrics";

        public static MetricsProvider Provider { get; set; }

        static Metrics()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (Provider != null) return;
            var appMetricsType = Type.GetType(AppMetricsProviderType, throwOnError: false);
            appMetricsType = appMetricsType ?? typeof(NullMetricsProvider);
            Provider = (MetricsProvider) Activator.CreateInstance(appMetricsType);
        }

        public static Task<string> Report()
        {
            return Provider.Report();
        }
    }
}