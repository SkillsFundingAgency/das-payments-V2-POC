using System.Diagnostics;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Payments.PocWebClient.Models;

namespace SFA.DAS.Payments.PocWebClient.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public JsonResult Start(string type)
        {
            var jobId = BackgroundJob.Enqueue(() => new DataLockJob().RunDataLock(null, type));
            return new JsonResult(new { ok = true, jobId });
        }
    }
}
