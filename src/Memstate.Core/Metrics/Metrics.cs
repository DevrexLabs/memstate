using System.Threading.Tasks;

namespace Memstate
{
    public class Metrics
    {
        public static MetricsProvider Provider { get; set; } = new NullMetricsProvider();

        public static bool AutoConfigure()
        {
            //assign Provider by searching for known providers
            //todo look for implementations using reflection
            //return true if successful
            return false;
        }

        public static Task<string> Report()
        {
            return Provider.Report();
        }
    }
}