using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Memstate.Host.Web.Controllers
{
    public class MetricsController : Controller
    {        
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var report = await Metrics.Report();
            return Content(report, "text/plain");
        }
    }
}