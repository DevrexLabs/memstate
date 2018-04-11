using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Memstate.Host.Web.Controllers
{
    public class MetricsController : Controller
    {
        private readonly MemstateSettings _settings;

        public MetricsController(MemstateSettings settings)
        {
            _settings = settings;
        }
        
        [HttpGet("metrics")]
        public async Task<IActionResult> Metrics()
        {
            var metrics = _settings.Metrics;

            var snapshot = metrics.Snapshot.Get();
            
            using (var stream = new MemoryStream())
            {
                await metrics.DefaultOutputMetricsFormatter.WriteAsync(stream, snapshot);

                var result = Encoding.UTF8.GetString(stream.ToArray());

                return Content(result, "text/plain");
            }
        }
    }
}